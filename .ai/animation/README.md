# Product walkthrough animation

Source for the walkthrough media used in the [README](../../README.md) and on the landing page:

| Output | Committed to | Used by |
|---|---|---|
| `techquiz-demo.gif` | `docs/media/` | README hero |
| `techquiz-walkthrough.mp4` + poster | `web/public/media/` | landing page "See it in action" |

The scene is a **Design Compiler** document (`.dc.html`) — a JavaScript-driven animation, 44 s at
1280×720, walking through sign-in → tracks → category → quiz → result, then revealing the architecture.
It is rendered to video/GIF because **GitHub markdown cannot execute JavaScript**, so the animation can
never be embedded live in a README.

## Files

```
Project workflow animation/
├── TechQuiz Walkthrough.dc.html   ← entry point; declares the scene + its props
├── techquiz-scene.jsx             ← the scene itself (Momentum tokens, screens, architecture, outro)
├── animations.jsx                 ← generic timeline engine (Stage, Sprite, useTime, easing)
└── support.js                     ← Design Compiler runtime
render.js                          ← deterministic frame capture → master MP4
derive.sh                          ← master MP4 → publishable GIF / MP4 / poster
```

## Re-rendering

Needs Node, `ffmpeg` on `PATH`, and network access (the runtime pulls React from unpkg and fonts from
Google Fonts). Playwright resolves from the repo's own `node_modules`.

```bash
cd .ai/animation
node render.js master.mp4 30   # ~6 min: 1320 frames at 1280x720
bash derive.sh                 # writes out/ — GIF, MP4, poster
```

Then copy the outputs to `docs/media/` and `web/public/media/`.

Render the master **once** at high quality and derive every cut from it — re-rendering per variant wastes
minutes for no gain. Flat UI compresses extremely well: the 44 s master lands around 1.4 MB at crf 18.

## How the capture works

Not a screen recording. `render.js` drives the runtime's own export protocol: it dispatches
`data-om-seek-to-time-frame` on the `svg[data-om-exportable-video-with-duration-secs]` element, which
pauses playback and pins the playhead to an exact timestamp, then screenshots that element and pipes the
PNG to ffmpeg. Output is therefore deterministic and reproducible.

## Gotchas

- **Serve over HTTP, never `file://`.** The runtime `fetch()`es the `.jsx` modules, which CORS blocks on
  `file://`. `render.js` starts its own throwaway server.
- **Element screenshots can land on an odd height** (1280×**721**); H.264 refuses odd dimensions, so the
  ffmpeg call crops to even with `crop=trunc(iw/2)*2:trunc(ih/2)*2`.
- **Set a base text colour on every top-level scene container.** `Stage` sets a background but no
  `color`, so any element that doesn't name its own colour inherits the *initial black* — invisible on
  this dark scene. Elements that set a colour explicitly look fine, so the symptom reads as "only the
  headings are broken". This shipped once by mistake and had to be re-rendered (#369). Diagnose by
  reading computed styles in the page, not by eyeballing frames.
- **`support.js` does not define `Stage`/`Sprite`/`useTime`** — those come from `animations.jsx`.

## Known inaccuracy

The per-track and per-category question counts shown in the animation (".NET — 118 questions",
"C#/.NET — 22 q", …) are invented placeholders and do not match the seeded bank (.NET actually holds 179;
Databases and Front-End 30 each). "4 tracks · 18 topics" is correct. Left as-is deliberately: the counts
change with any content expansion, so they should be corrected and re-rendered in one pass.

## Not tracked

The design tool's export archive (`.zip`), `uploads/` and `.thumbnail` are excluded — verified unused by
the render (the scene references no images), and the archive merely duplicates the extracted folder.
