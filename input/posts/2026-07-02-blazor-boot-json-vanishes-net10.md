---
Title: When .NET 10 Deleted the File We Were Proudly Preloading
Description: We shipped a preload hint for blazor.boot.json to fix a boot waterfall, and it worked beautifully — until a .NET 10 upgrade quietly removed that file from existence and turned our own optimization into a 404.
Published: 2026-07-02
Tags:
  - Blazor
  - ASP.NET Core
  - WebAssembly
  - PWA
Author: Abdulawwal Intisor
---

A few weeks ago, I wrote about shaving nearly a second off ZikFash's boot time by preloading `blazor.boot.json` before the browser even asked for it. It worked. I was pleased with myself. Then .NET 10 landed, and that exact same line of code turned into a 404 error in my own console.

This is the short version of that story: an optimization that quietly expired underneath me, and why that's a completely normal thing to happen when you build on a framework that's still moving.

---

## The Symptom

After bumping ZikFash to .NET 10, the browser console started logging this on every single load:

```http
GET http://localhost:5262/_framework/blazor.boot.json 404 (Not Found)
```

Nothing crashed. The app still worked. But it added an unnecessary request to the waterfall and littered the logs during both development and production runs — exactly the kind of noise that makes you distrust your own console the next time something *actually* breaks.

## The Root Cause

A bit of digging into .NET 10's WebAssembly boot architecture explained everything: **`blazor.boot.json` is no longer generated as a separate static file.** Starting with .NET 10, the boot manifest is compiled and inlined directly inside `dotnet.js` at build time, specifically to cut down on requests and streamline WebAssembly startup.

Which means the very optimization I was proud of — the hardcoded preload hint sitting in `<head>` —

```html
<link rel="preload" href="_framework/blazor.boot.json" as="fetch" crossorigin="anonymous" />
```

— was asking the browser to preload a file that no longer exists anywhere on disk or in the server's static assets. The framework I was optimizing for had moved the goalposts, and my code hadn't noticed.

It's a good reminder that "optimize for the framework's internals" comes with an expiration date. The framework owes you a stable public API. It owes you nothing about *how* it gets there internally, and .NET 10 changed exactly that.

---

## The Fix

**Cleaning up `index.html`.** The obsolete preload line came out entirely, leaving the two hints that are still doing real work:

```diff
     <!-- Preload critical WASM/DLLs to bypass the Blazor sequential boot cascade -->
     <link rel="preload" href="_framework/dotnet.native.wasm" as="fetch" crossorigin="anonymous" />
     <link rel="preload" href="_framework/System.Private.CoreLib.wasm" as="fetch" crossorigin="anonymous" />
-    <link rel="preload" href="_framework/blazor.boot.json" as="fetch" crossorigin="anonymous" />
 </head>
```

**Auditing the service workers.** Before calling it done, I checked both `service-worker.js` and `service-worker.published.js` for any hardcoded reference to `blazor.boot.json` in their static cache manifests — the kind of thing that could quietly break offline caching down the line. Neither one had any. Both rely on the dynamically generated `service-worker-assets.js` for framework asset caching, so there was nothing left to clean up there.

**Verifying the build.** A full `dotnet build` from the workspace root came back clean: 0 errors, 0 warnings.

---

## Why This Was Worth a Whole Post

It's a two-line fix, but the lesson underneath it is bigger than two lines. Every time you optimize around a framework's *implementation details* rather than its *public contract*, you're borrowing against a future upgrade. That's not a reason to avoid these optimizations — the preload hint genuinely bought ZikFash real load-time savings, as covered in the [previous investigation into that 8-second boot delay](/posts/2026-06-18-deciphering-blazor-wasm-boot-performance/). It's a reason to expect them to need revisiting, and to actually re-audit `index.html`, your service workers, and your build output every time you jump a major framework version — instead of assuming yesterday's fix is still true today.

The console is now clean, the network waterfall is prioritizing the WASM files that are actually preloaded, and ZikFash's performance layer is aligned with how .NET 10 actually boots — not how .NET 8 used to.
