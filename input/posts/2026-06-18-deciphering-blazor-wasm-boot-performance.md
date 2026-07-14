---
Title: Deciphering the 2-Second Silence - A Blazor WASM Load-Time Investigation
Description: Chrome Lighthouse said 8.52 seconds to first paint. Here's how we hunted down a sequential boot waterfall, a 3MB XML library nobody asked for, and a redundant NuGet package — and got ZikFash's shell rendering in under a second.
Published: 2026-06-18
Tags:
  - Blazor
  - WebAssembly
  - Performance
  - ASP.NET Core
  - PWA
Author: Abdulawwal Intisor
---

Chrome Lighthouse doesn't do insults, but the number it gave me felt like one anyway.

**Largest Contentful Paint: 8.52 seconds.**

Eight and a half seconds is an eternity for a modern PWA — it's the gap between a user onboarding and a user deleting the app before they've even tried it. I was building ZikFash, a "premium digital record book" for professional tailors, and I was failing the one requirement that mattered before any feature did: speed.

Even after I refactored the UI to render the shell — the header, the logo — instantly, there was still a frustrating two-second silence after it. The logo would appear, and then nothing. Just a spinner, doing nothing useful, for two whole seconds. So I decided to actually decipher what those two seconds were spent on. This is that investigation.

---

## The Staircase Nobody Warned Me About

I opened the Network tab and hit refresh, expecting a burst of parallel downloads. Instead I got a staircase.

Blazor WASM's default boot sequence has trust issues — it refuses to ask for anything until the thing before it has fully answered:

1. `index.html` loads.
2. It loads `blazor.webassembly.js`.
3. That script fetches `blazor.boot.json`.
4. Only *after* the manifest is parsed does it start requesting the 50+ assemblies (`.wasm` and `.dll` files) that make up the actual app.

That's a sequential bottleneck dressed up as a boot process. My connection was 100Mbps, but for the first second and a half, the browser was using maybe 1% of that bandwidth, because it was waiting for one script to tell it what to ask for next — a relay race where the runners walk between handoffs.

The fix didn't require waiting for JavaScript to tell the browser what mattered most. I already knew what mattered most. I added `<link rel="preload" as="fetch" crossorigin="anonymous">` hints to `index.html` for the big three:

- `dotnet.native.wasm` — the engine.
- `System.Private.CoreLib.wasm` — the heart.
- `blazor.boot.json` — the map.

That alone put the full network pipe to work from the first millisecond instead of the second-and-a-half mark. The engine started downloading in parallel with the initial HTML parse, and we clawed back nearly 800ms of pure "thinking time" from three lines of HTML.

---

## Hunting the 3MB Library Nobody Invited

With the loading *order* fixed, it was time to look at the loading *size*. I ran a quick check on the `_framework` folder and nearly fell out of my chair.

**`System.Private.Xml.wasm`: 2.95 MB.**

ZikFash is a modern app. We use JSON. We use REST. We use Supabase. There is not a single line of XML anywhere in the codebase. So why was every user downloading a 3MB XML library on first load?

An hour of tracing the dependency graph turned up **transitive bloat**. We were using the Supabase 1.1.1 SDK, which pulls in `Postgrest-csharp` and `Realtime-csharp`, both built on Newtonsoft.Json. Newtonsoft is a legendary library, but it's a Swiss Army knife from an older era — it ships with built-in converters for `XmlNode` and `XDocument`. Because those references are baked directly into the assembly, the .NET trimmer wouldn't touch them. It saw a reference to XML and played it safe: keep the whole 3MB stack, just in case.

That wasn't the only stowaway. `System.Reactive.wasm` was adding another 1.27 MB, entirely in service of "realtime" events we weren't even using yet.

---

## Reaching for the Full Trim

The standard Blazor build runs with `TrimMode=partial` — it only trims assemblies that explicitly declare themselves safe to trim, and most legacy libraries never make that promise. So I went further:

```xml
<TrimMode>full</TrimMode>
```

Full trimming assumes that if your code never actually calls something, it's dead weight, full stop — no benefit of the doubt for "potential" reflection-based references. The risk is real: if Supabase used reflection somewhere to find an XML converter at runtime, the app would break in a way tests might not catch. The reward was just as real: almost 5MB shaved off the initial payload.

I also switched on **invariant globalization**. ZikFash doesn't need Thai or Japanese sorting rules, so the 1MB ICU data file went with it.

---

## Deleting the Package We'd Already Replaced

Digging through `Program.cs`, I found one more piece of baggage: `Blazor.LocalStorage.WebAssembly`, still registered as a service.

We'd spent the previous few days writing a custom IndexedDB and `localStorage` JS bridge specifically to handle shop-scoped data for ZikFash (that's [its own story](/posts/2026-06-01-offline-first-zikfash-sync-engine/)). The NuGet package was doing the exact same job our bridge already did, just with an extra C#-to-JS round trip we'd already engineered away. I deleted it, removed the registration, and the assembly count dropped again.

---

## Where We Landed

Put together, the changes were transformative:

- **The sequential waterfall broke.** Assets started preloading instantly instead of waiting their turn.
- **The XML ghost got exorcised.** `System.Private.Xml` was finally pruned from the bundle.
- **The boot order got refined.** The shell — header and logo — now renders immediately, with a "Loading your record book…" state scoped only to the content area.

**Final numbers:**
- LCP dropped from 8.52s to **under 1 second** for the shell.
- Payload shrank by roughly 4.5MB uncompressed.
- The app now *feels* instant — the brand appears in under 500ms, and the data is ready by the time a tailor has even looked at the menu.

---

## What I'd Tell the Next Developer

If your Blazor app is slow, don't blame the framework first. Blazor is fast; your dependency graph is probably just carrying passengers you didn't invite.

- Audit your transitive DLLs. If you spot `System.Xml` or `System.Data` in the output and you've never typed those words, find out who invited them.
- Break the sequential cascade — preload `blazor.boot.json` and `dotnet.native.wasm` before JavaScript ever asks for them.
- Be brave with `TrimMode=full`, but only if you have a test suite that would actually catch a reflection-based crash.

High-performance software isn't about what you bolt on. It's about having the discipline to go back and remove what you don't need — even after it's already shipped.

One more twist to this story, though: a few weeks after this fix landed, that same `blazor.boot.json` preload hint I was proud of turned into a brand-new bug, courtesy of .NET 10. More on that next.
