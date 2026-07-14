---
Title: Building ZikFash Offline-First - Engineering Trust When the Network Isn't There
Description: How we rebuilt ZikFash's data layer from a fragile "save and hope" hack into a conflict-aware, IndexedDB-backed sync engine that behaves the same on fibre in Accra as it does with zero bars in a crowded market.
Published: 2026-06-01
Tags:
  - Blazor
  - PWA
  - IndexedDB
  - Offline-First
  - Architecture
Author: Abdulawwal Intisor
---

My mother runs a tailoring shop. That's not a fun fact — it's the whole reason ZikFash exists. I've watched her write "Half Waist" and "Shoulder Width" into a paper record book for years, and I've watched the power cut mid-measurement more times than I can count. Any app that's supposed to replace that paper book has to survive the exact moment the internet doesn't.

In a normal cloud-first app, that moment looks like a spinner, a timeout, and eventually a red banner that says "Failed to save." For a tool meant to earn a tailor's trust, that's not a bug. That's a resignation letter.

That single scenario is the reason ZikFash's data layer doesn't look anything like it did a few months ago. We tore out a "save to local and hope" system and rebuilt it into something that behaves identically whether you're on fibre in Accra or standing in a market with zero bars: local-first, always, with the cloud treated as backup — not a dependency.

This is the story of that rebuild: the storage ceiling we hit, the sync conflicts we had to actually solve instead of hand-wave away, and one service worker bug that made icons vanish for reasons that had nothing to do with icons.

---

## Hitting the Wall: localStorage's Invisible Ceiling

We started, like everyone does, with `localStorage`. It's the "hello world" of persisting anything in a browser — dead simple, synchronous, works everywhere.

It also has a hard 5MB cap, and it blocks the main thread on every write. Neither of those facts matter until they suddenly matter a lot. As tailors added more customers, more orders, more measurements, two things happened at once:

1. **The wall.** Get close enough to 5MB of JSON and the browser throws `QuotaExceededError`. Your "reliable offline app" just stopped being reliable.
2. **The jank.** Every `localStorage.setItem()` freezes the UI while it writes to disk. On a growing customer list, that's a visible stutter — exactly the kind of thing that makes a premium-feeling app suddenly feel cheap.

The fix wasn't clever, it was necessary: move to **IndexedDB**. Asynchronous, effectively unbounded, built for exactly this job. The actual engineering problem wasn't the migration — it was doing it without a single existing user noticing.

We wrote a silent migration that runs once, on boot:

- Check for a `zf_migrated_v1` flag.
- If it's missing, read every shop-specific bucket out of `localStorage` (`zikfash_customers_v1_IntiCloths`, and so on).
- Parse it, write it into the new IndexedDB `Customers` store.
- Only flip the flag once the IndexedDB write is confirmed.

A tailor opens the app one morning and their entire customer history has quietly moved into a bigger, faster vault — no changelog, no popup, no dropped record. That's the bar for any infrastructure change: it should be invisible to the person actually using the tool.

---

## Two Tablets, One Truth

Storage was the easy half. The hard half of any offline-first system is what happens when two devices go offline separately and both come back online — because now you have two versions of "the truth," and somebody has to decide which one wins.

We ruled out the lazy options fast:

- **Cloud always wins** deletes whatever the tailor just did offline on the tablet.
- **Local always wins** deletes whatever changed on the phone while the tablet was out of signal.

Neither is acceptable when the data is a paying customer's measurements. So we built a **record-level merge**:

- Every `Customer` and `Order` carries an `UpdatedAt` timestamp, touched the instant a field changes.
- Every `ShopConfig` carries a `LastSyncAt` — the last moment *this specific device* successfully spoke to the cloud.

When the tailor hits sync, we run a pre-flight check: is the cloud's last-updated timestamp newer than this device's `LastSyncAt`? If yes, someone else moved first — and instead of aborting or blindly overwriting, we merge record by record, keeping whichever version has the newer timestamp.

It's not a fancy CRDT. It's honest, boring, and it handles the 99% case that actually happens in a real shop: two people editing different customers on different days.

---

## The Freeze No One Was Supposed to See

Here's the part that had nothing to do with sync logic and everything to do with Blazor's plumbing. JS interop — Blazor WebAssembly's bridge to JavaScript — is fantastic until you push real volume through it. Every crossing means C# serializes to a string, that string copies across the WASM boundary, and JavaScript parses it back into an object.

Do that with a full merge of a few megabytes of customer data and the UI freezes for close to a second. For an app whose entire pitch is "instant," a full second of nothing is a broken promise.

The fix was to stop pushing *data* across the boundary and only push the *diff*. C# sends only the newly changed records. JavaScript pulls the full existing dataset straight from IndexedDB — no bridge involved — and merges everything itself using a `Map` for O(n) lookups:

```javascript
const mergedMap = new Map();
cloudData.forEach(c => mergedMap.set(c.Id, c));
localData.forEach(l => {
    const existing = mergedMap.get(l.Id);
    if (!existing || new Date(l.UpdatedAt) > new Date(existing.UpdatedAt)) {
        mergedMap.set(l.Id, l);
    }
});
```

The merged result gets written back to IndexedDB and handed to C# as a single "here's your new state" call. One second of freeze became about twenty milliseconds nobody notices. Same merge logic, completely different experience — just by being deliberate about what actually needs to cross the C#/JS border.

---

## The Case of the Disappearing Scissors Icon

This one took longer to find than it should have, mostly because the symptoms lied to us. The Material icons throughout the UI — scissors, rulers, the small visual language of the app — would randomly vanish, even on a perfectly good connection.

The service worker was caching Google Fonts requests with `ignoreSearch: true`, a setting that's usually correct for local assets, where you want to ignore cache-busting query strings like `app.css?v=1`. But Google Fonts identifies *which* font you're asking for through the query string:

- `fonts.googleapis.com/css2?family=Lato`
- `fonts.googleapis.com/css2?family=Material+Symbols+Rounded`

With `ignoreSearch: true`, the service worker treated both as the *same URL*. Whichever one got cached first won, permanently. If Lato loaded first, every later request for Material Symbols got served Lato's CSS instead, the browser failed to parse it as an icon font, and the icons just disappeared.

The fix: stop treating every cached asset the same way. Local, versioned assets keep `ignoreSearch: true`. Anything external, where the query string *is* the identity, gets `ignoreSearch: false` plus a stale-while-revalidate strategy — serve the cached copy instantly, then quietly refetch in the background so the next load is fresh.

---

## Knowing You're Offline Is Half the Job

A good offline app doesn't just keep working when the network drops — it tells you it knows. We built a small `NetworkStatusService` that listens for the browser's `online`/`offline` events and surfaces a quiet, unapologetic banner: *"Working Offline — changes saved locally."*

It sounds like a small thing. It isn't. It's the difference between a tailor trusting the tool enough to finally put the paper record book away, and a tailor keeping that book in a drawer "just in case." The `StorageService` also checks this status before attempting a sync — if you're in a market basement with zero signal, we're not going to waste your battery pretending otherwise.

---

## What's Still Ahead

The architecture holds up fine for thousands of records, but we already know where the next cracks will show:

- **Tombstones.** Right now, deleting a customer offline can get that customer re-imported from another device during sync. We need an `IsDeleted` flag that syncs the *act* of deletion, not just the absence of a record.
- **Differential sync.** Sending the full list to Supabase works today. It won't scale forever — sending only the dirty records is next.
- **A real conflict UI.** Last-write-wins is fine for measurements. It won't stay fine forever for payments — some conflicts deserve a human decision, not an algorithm's.

## The Actual Lesson

Offline-first isn't a package you install. It's a mindset that treats local storage as the source of truth and the cloud as a layer that catches up when it can. It costs you JS interop tricks, timestamp math you have to get right, and service worker behavior you have to actually understand instead of copy-paste.

But when a tailor in a busy market saves a measurement and it just works — no spinner, no error, no drama — all of that complexity earns its keep. That's the whole point.

Next up: we go chasing an 8-second load time that had absolutely no business existing, and it gets uncomfortably detective-y.
