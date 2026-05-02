# INTITECH — Complete Design System Handoff Prompt
# Use this prompt verbatim in any AI design tool, Figma plugin, or new Claude conversation
# Version 1.0 · April 2026

---

## MASTER PROMPT

You are building the complete visual identity system for INTITECH — the personal brand of Abdulawwal Intisor Mawupego, a backend engineer from Akure, Nigeria. This is not a generic tech brand. Every design decision must trace back to the identity philosophy below.

---

## IDENTITY FOUNDATION

**Name:** INTITECH  
**Person:** Abdulawwal Intisor Mawupego  
**Tagline:** "I build what's needed."  
**Philosophy:** Pattern recognition from proximity. Every product built from being close to the problem — not from market research.  
**Origin:** Akure, Nigeria. Muslim. Final-year Software Engineering student at FUTA.  
**Ambition:** Compete globally from the trenches. AgTech direction long-term. Wageningen University target.  

**The name cipher:**  
IN·nova·TI·ve·TECH = INTITECH  
The brand name is hidden inside the philosophy. Those who look closely see it.

---

## AESTHETIC DIRECTION

Dark. Noisy. Archaic architecture.  
- Background: near-void black (#080705). Not navy. Not charcoal. Void.  
- Texture: SVG fractalNoise grain overlay at opacity 0.05, mix-blend-mode overlay  
- Feel: ancient manuscript meets live system dashboard. Roman inscription energy. A scribe who builds distributed systems.  
- NOT: cyberpunk, pixel art retro, minimal white space startup, generic dark mode  

---

## COMPLETE COLOR SYSTEM

### Soil (dark backgrounds)
```
--void:       #080705   /* page background */
--deep-earth: #0f0d09   /* primary surface */
--topsoil:    #1c1a13   /* card backgrounds */
--sediment:   #2e2b22   /* all borders/dividers */
--ash:        #6b6355   /* muted/secondary text */
```

### Clay (warm text/surface)
```
--parchment:  #c9b99a   /* primary body text */
--script:     #e8dcc8   /* headings */
--raw-clay:   #a08060   /* mid-tone */
--terracotta: #7a4f35   /* warm accent */
```

### Heat (primary actions)
```
--ember:      #c8612a   /* primary CTA, links, active states */
--ember-dim:  #4a2410   /* ember hover backgrounds */
--gold:       #a8873a   /* INTITECH wordmark, logo text */
--dry-grass:  #8a7a2e   /* bridge: heat meets growth */
```

### Growth (agro greens — ONLY for live/success states)
```
--deep-root:  #1a2e1a   /* growth backgrounds */
--moss:       #3d5c35   /* success states */
--canopy:     #5a8a48   /* live/online indicators */
--sprout:     #8ab87a   /* positive highlights */
--dew:        #c2ddb8   /* text on green backgrounds */
```

**Color rules:**
1. Growth greens appear ONLY for live system states — never decoratively
2. Ember is the only interactive color — all CTAs, all hover states, all links
3. Gold is reserved for the INTITECH wordmark and logo only
4. Never use pure white (#ffffff) — use --script (#e8dcc8) instead

---

## TYPOGRAPHY SYSTEM

```
Display:   Cinzel (Google Fonts) — weight 400/700/900
           Used for: INTITECH wordmark, all section headings, all-caps labels
           
Body:      IM Fell English (Google Fonts) — regular + italic
           Used for: all prose, taglines, blog content, pull quotes
           
Mono:      JetBrains Mono (Google Fonts) — weight 300/400/500
           Used for: code blocks, API metrics, nav links, labels, timestamps, hex values
```

**Type rules:**
- All section headers: Cinzel, uppercase, letter-spacing 0.05em minimum
- All technical data: JetBrains Mono always
- Body prose: IM Fell English, line-height 1.85-1.95
- Never mix Cinzel and IM Fell English in the same line

---

## THE LOGO MARK — KARAKUL CAP

### What it is
The Karakul cap is a traditional cap worn across Muslim and Central/West African communities. Abdulawwal wears one daily and is recognized by it. It is his visual trademark.

### Mark geometry (from technical blueprint)
```
Shape: front-facing trapezoid
- Wider at top, slight taper toward base
- Flat crown with single folded rim curve at top
- Single vertical seam line — subtle, centered
- Clean geometric silhouette — NO texture, NO fluff, NO fur pattern
- Plain matte surface

SVG path reference:
Body:    M -52 90 L -58 12 Q -58 0 -44 0 L 44 0 Q 58 0 58 12 L 52 90 Z
Rim:     M -44 0 Q -44 -10 0 -12 Q 44 -10 44 0
Seam:    vertical line x1="0" y1="-8" x2="0" y2="90"
```

### The letterform on the cap — CRITICAL
**"IN" for favicon/small sizes (under 64px)**  
**"INTI" for display/hero sizes (above 64px)**

The letters must NOT be a font. They must be drawn using **leaf venation geometry** — the branching network pattern found in plant leaves.

### Leaf venation letterform specification:
```
Method: construct I and N (and T, I for INTI) using branching vein lines
- Start with a central midrib (main stem line) for each letter stroke
- Add secondary veins branching off at 30-45 degree angles
- Add tertiary micro-veins branching from secondary veins
- Veins taper in stroke-width from thick (midrib ~2px) to thin (tertiary ~0.4px)
- Letters should be readable as letterforms from the vein structure
- The branching follows Fibonacci ratios in spacing

Color of letterform on cap:
- On dark cap: use --dry-grass (#8a7a2e) or --sprout (#8ab87a)
- On light cap: use --deep-root (#1a2e1a) or --moss (#3d5c35)
- The green on the dark cap = plant growing from dark earth = the whole brand narrative
```

### Logo states
```
1. Mark Dark (primary):
   Cap fill: --topsoil (#1c1a13)
   Cap border: --sediment (#2e2b22) at 0.5px
   Rim: --sediment (#2e2b22)
   Vein letters: --dry-grass (#8a7a2e) 
   Base shadow: ellipse, --void, opacity 0.6
   Background: --void (#080705)

2. Mark Ember (hover/active):
   Cap fill: --topsoil (#1c1a13)
   Cap border: --ember (#c8612a) at 0.8px
   Rim: --ember-dim (#4a2410)
   Vein letters: --sprout (#8ab87a)
   Glow: ember drop-shadow at opacity 0.2
   Background: --void (#080705)

3. Mark Light (on light backgrounds):
   Cap fill: --parchment (#c9b99a)
   Rim: --raw-clay (#a08060)
   Vein letters: --deep-root (#1a2e1a)
   Background: --script (#e8dcc8)

4. Mark Gold (special/featured):
   Cap fill: --topsoil (#1c1a13)
   Cap border: --gold (#a8873a)
   Vein letters: --gold (#a8873a)
   Background: --void (#080705)
```

### Full lockup composition
```
[Mark] | [vertical rule 1px --sediment] | [INTITECH wordmark] 
                                           [tagline in IM Fell English italic]

Wordmark rendering:
- "INTI" in Cinzel 900 color: --ash (#6b6355) — the hidden part
- "TECH" in Cinzel 900 color: --script (#e8dcc8) — the visible declaration
- No space between INTI and TECH
- Letter-spacing: 0.05em

Tagline: "I build what's needed."
- IM Fell English, italic, size ~30% of wordmark height
- Color: --ash (#6b6355)
```

---

## PLANT GEOMETRY ORNAMENT SYSTEM

These ornaments are used as decorative elements across the portfolio and blog. They replace generic dividers and section breaks with nature geometry.

### Ornament A — Fibonacci Spiral
```
Use: hero section background, large section ornaments
Construction:
- Quarter-circle arcs following Fibonacci sequence: 1,1,2,3,5,8,13,21
- Each arc in a new square sized to the next Fibonacci number
- Stroke only, no fill
- Stroke color: --sediment (#2e2b22) at opacity 0.4
- At key joints: small circle dot, radius 1px, --ember (#c8612a) at opacity 0.3
- Size range: 200px to 600px depending on context
```

### Ornament B — Branching Fractal (the signature ornament)
```
Use: section dividers left/right margins, blog post margins, corner decorations
Construction:
- Central trunk line, vertical or at 15-degree angle
- Level 1 branches: fork at 35-40 degrees, length = trunk × 0.7
- Level 2 branches: fork at 30-35 degrees, length = L1 × 0.7
- Level 3 branches: fork at 25-30 degrees, length = L2 × 0.7
- Level 4 micro-twigs: length = L3 × 0.6 (optional, for large sizes)
- Stroke width tapers: trunk 1.5px → L1 1px → L2 0.7px → L3 0.4px
- Stroke color: --sediment (#2e2b22) at opacity 0.6
- At terminal tips: tiny circle 0.8px radius, --canopy (#5a8a48) at opacity 0.5
- Size range: 80px to 200px for margins

Placement rule: always on left AND right side mirrored, like manuscript illustrations
```

### Ornament C — Leaf Vein Single Leaf
```
Use: bullet points, list markers, small accents, favicon background
Construction:
- Outer leaf silhouette: pointed oval, slight asymmetry
- Central midrib: single stroke, thick to thin
- Secondary veins: 6-8 pairs branching at 45 degrees
- Tertiary veins: fine lines between secondary
- Stroke only, no fill
- Colors: --sediment to --moss depending on context
- Sizes: 8px (bullet), 24px (accent), 48px (featured)
```

### Ornament D — Phyllotaxis dots (portfolio background texture)
```
Use: subtle background texture on hero section, skills wall
Construction:
- Points arranged in Fibonacci spiral pattern (137.5 degree golden angle)
- Each point: circle radius 1px
- Color: --sediment (#2e2b22) at opacity 0.25
- Density: ~150 points per 400px square
- Fade to transparent at edges (radial gradient mask)
```

### Ornament E — Arch divider (existing, keep)
```
Current implementation: horizontal line with centered text label
Format: ✦ [SECTION TITLE] ✦
Font: Cinzel, 0.62rem, letter-spacing 0.4em, color --ghost (#2e2b22)
```

---

## PORTFOLIO SITE REBUILDING SPECIFICATION

**Current problem:** Looks vibecoded. Generic hero copy. "Student" framing undersells. Personality missing.

### Hero Section — exact copy
```
Eyebrow (JetBrains Mono, --ember):
// IN·nova·TI·ve·TECH

Name (Cinzel 900, two-line):
INTI  ← color: --ash (#6b6355)
TECH  ← color: --script (#e8dcc8)

Divider: ember gradient line 4rem wide

Tagline (IM Fell English italic, --faded):
"I build what's needed.
Not what's trending. Not what's been built.
What's needed."

Sub-tagline (JetBrains Mono, --ash, small):
Pattern recognition from proximity · Akure, Nigeria

CTAs:
[VIEW WORKS]  ← btn-primary, ember fill
[INSPECT SYSTEM] ← btn-ghost
```

### About Section — exact copy
```
Opening (IM Fell English italic):
"Most developers build for markets.
I build from proximity."

Body paragraph 1:
"Final-year Software Engineering student at FUTA — 
with real systems already in production. 
Self-taught before formal education. 
Building from first principles, for real problems."

Body paragraph 2:
"Career goal: become a highly skilled C# engineer 
building fast, resource-efficient systems. 
Not just writing code — engineering things that endure."

Body paragraph 3:
"Microsoft Learn Student Ambassador. 
Assistant General Secretary, AMSA Nigeria. 
Writer — at the intersection of technology, 
philosophy, and the Nigerian condition."

NO mention of "student" as primary identity
NO generic "crafting scalable solutions" language
```

### Skills Wall — exact instruction
```
Label: ✦ Inscribed Disciplines ✦
Display as stone inscription grid, NOT tag cloud
Brightness tiers:
- Core (C#, ASP.NET Core, EF Core, SQL Server): color --parchment
- Primary (REST API, Clean Architecture, SignalR, Hangfire): color --faded  
- Secondary (React, Blazor, WebRTC, Chrome MV3): color --ash/--ghost
```

### Live System Panel — exact specification
```
Panel label: LIVE SYSTEM STATUS (top-left notch label)
Status dot: --canopy green, pulse animation

Metrics (4 cells, 2x2 grid):
- Response Time: live value in ms, color --canopy
- Requests Today: incrementing counter
- Uptime: 99.9%, color --canopy  
- Active Projects: 4, color --gold

Endpoint tester:
- Cycles through: /api/v1/projects, /api/v1/health, /api/v1/skills
- Response displayed in mono green on dark bg
- Execute button styled with ember border

Fixed bottom-right ticker:
SYS [latency]ms  MEM [%]  █ (blinking cursor)
```

---

## BLOG SITE SPECIFICATION (blogs.intitech.dev)

### Statiq.Web override files needed:
```
input/_layout.cshtml    — full archaic layout with grain
input/_head.cshtml      — meta, OG tags, fonts
input/_navbar.cshtml    — nav with portfolio link
input/_post-header.cshtml — post masthead
input/_footer.cshtml    — footer
input/index.cshtml      — homepage post list
input/scss/_variable-overrides.scss — kill Bootstrap defaults
```

### Post list item structure:
```
[TAG PILLS in mono]
[POST TITLE in Cinzel]
[EXCERPT in IM Fell English italic]
[DATE in JetBrains Mono, right-aligned]
Left border: 2px ember on hover
```

### Branching ornament placement on blog:
```
- Left margin of each post: Ornament B at 60% opacity
- Right margin mirrored: Ornament B at 40% opacity  
- Between post sections (after h2): Ornament C leaf, centered, 24px
- Post header background: subtle Ornament D phyllotaxis dots
```

---

## URL SHORTENER SPECIFICATION (i.intitech.dev)

### Stack (migrated from MVC):
```
Framework: ASP.NET Core Minimal API (.NET 8)
Database: SQLite (file: smallurl.db)
Encoding: Hashids.net — salt from environment variable NEVER hardcoded
Auth: X-Api-Key header on protected routes
```

### Endpoints:
```
GET  /              → health check JSON
GET  /{slug}        → 301 redirect + log click  
GET  /api/stats     → all links with click counts (public)
GET  /api/stats/{slug} → single link stats (public)
POST /api/shorten   → create short link (protected)
DELETE /api/links/{slug} → delete link (protected)
```

### Models:
```csharp
Link  { Id, OriginalUrl, Label, CustomSlug?, CreatedAt, Clicks[] }
Click { Id, LinkId, ClickedAt }
```

### Planned branded slugs:
```
i.intitech.dev/amsa   → AMSA blog post
i.intitech.dev/fin    → FinSight blog post  
i.intitech.dev/blog   → blogs.intitech.dev
i.intitech.dev/gh     → GitHub profile
```

---

## SEO CONFIGURATION

### JSON-LD (paste before </body>):
```json
{
  "@context": "https://schema.org",
  "@graph": [
    {
      "@type": "Person",
      "@id": "https://intitech.dev/#person",
      "name": "Abdulawwal Intisor Mawupego",
      "alternateName": ["Intitech", "Intisor Abdulawwal", "Abdulawwal Intisor"],
      "url": "https://intitech.dev",
      "jobTitle": "Backend Engineer",
      "nationality": "Nigerian",
      "address": { "addressLocality": "Akure", "addressCountry": "NG" },
      "alumniOf": { "name": "Federal University of Technology Akure" },
      "sameAs": [
        "https://github.com/intisor",
        "https://x.com/intitechdev",
        "https://linkedin.com/in/intitechdev",
        "https://intitech.substack.com"
      ]
    }
  ]
}
```

### Required root files:
```
sitemap.xml  — submit to Google Search Console
robots.txt   — Allow: / with sitemap reference
og-image.png — 1200×630px for social previews
favicon.svg  — Karakul cap mark SVG
```

---

## COMPLETE FILE DELIVERABLES CHECKLIST

### Already built (from this session):
```
✅ intitech.dev/index.html — portfolio SPA
✅ coming-soon.html — pre-launch page
✅ seo-head.html — complete meta + JSON-LD
✅ sitemap.xml
✅ robots.txt
✅ blog/input/_layout.cshtml
✅ blog/input/_head.cshtml
✅ blog/input/_navbar.cshtml
✅ blog/input/_post-header.cshtml
✅ blog/input/_footer.cshtml
✅ blog/input/index.cshtml
✅ blog/input/scss/_variable-overrides.scss
✅ blog/input/posts/2025-04-11-amsa-reporting.md
✅ blog/.github/workflows/deploy.yml
✅ smallurl-api/Program.cs (Minimal API migration)
✅ smallurl-api/Data/AppDbContext.cs
✅ smallurl-api/Models/Models.cs
✅ INTITECH-Brand-Handoff.docx
✅ intitech-logo-system.svg (first draft)
```

### Still needed:
```
⬜ Logo mark SVG — leaf vein "IN" on cap (favicon version)
⬜ Logo mark SVG — leaf vein "INTI" on cap (display version)  
⬜ Ornament B SVG — branching fractal (left/right margins)
⬜ Ornament A SVG — Fibonacci spiral (hero background)
⬜ Ornament C SVG — single leaf (bullet/accent)
⬜ Ornament D SVG — phyllotaxis dots (background texture)
⬜ Portfolio hero copy rewrite (anti-vibecode)
⬜ OG image 1200×630px
⬜ favicon.svg
```

---

## WHEN BUILDING — RULES TO NEVER VIOLATE

1. Never use pure white. Use --script (#e8dcc8)
2. Never use generic green (#4ade80). Use --canopy (#5a8a48)
3. Never write "crafting scalable solutions" or any similar generic copy
4. Never use the word "student" as primary identity descriptor
5. Growth greens are ONLY for live/online/success states — never decoration
6. The cap mark letterform is ALWAYS leaf vein geometry — never a font
7. Grain texture is always present — it's structural not optional
8. Cinzel is always uppercase — never sentence case
9. IM Fell English body text is always at minimum line-height 1.85
10. The INTI/TECH color split in the wordmark is always maintained — INTI dimmer, TECH brighter

---

## QUICK REFERENCE — WHO IS THIS FOR

When you're unsure about a design decision, ask:
"Does this feel like it was built by someone who:
- Gets angry at fintech clones
- Built Zikfash because of his mum's tailoring shop  
- Wears a Karakul cap
- Is reaching for Wageningen University
- Says 'I build what's needed' and means it"

If the answer is no — rebuild it.

---
*INTITECH Brand System · Abdulawwal Intisor Mawupego · intitech.dev · MMXXV*
