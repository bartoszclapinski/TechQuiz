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
bash derive.sh                 # writes out/ — GIF, MP4, poster (+ an unpublished WebP for size comparison)
```

Then copy the outputs to `docs/media/` and `web/public/media/`.

Render the master **once** at high quality and derive every cut from it — re-rendering per variant wastes
minutes for no gain. Flat UI compresses extremely well: the 44 s master lands around 1.5 MB at crf 18.

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

## Keep the numbers honest

The scene's catalogue figures (track/category question counts, quiz length, and the result fraction) are
real and must stay that way — this is a portfolio piece, so a wrong count is worse than no count.
User-specific values (per-category best scores, the "Highest score" card, time, "+5%", the login hero's
streak/Skill IQ/accuracy, the sample question) are illustrative, mirroring the static
`web/src/features/auth/auth-hero.tsx`. Anything that changes the seeded bank means editing
`techquiz-scene.jsx` and re-rendering:

| Shown in the scene | Source of truth |
|---|---|
| Track cards — `topics` and `q` | `DataSeeder.cs` + `Seed/Data/*Questions.cs` |
| `.NET` category cards — `q` per category | the per-category lists in `Seed/Data/*Questions.cs` |
| Quiz header `C#/.NET · 3 / 30` | a quiz is **every** question in its category — `StartQuizCommandHandler` does no `.Take()` |
| Result `26 / 30 correct` at `87%` | must divide out to the percentage the scene animates to |

As of the last render: 4 tracks over 18 categories, **269** questions — .NET 179 (30/30/30/30/29/30),
Databases 30, Front-End 30, Engineering Practices 30.

Count the *list entries*, not the factory methods — a file can define a question that no category list
references.

```bash
# from the repo root — counts list entries only, not the factory signatures
cd src/TechQuiz.Infrastructure/Persistence/Seed/Data
grep -cE '^\s+Q[0-9]+_\w+\(categoryId\),?$' *.cs
```

That prints one count per file (currently 30 everywhere except `UnitTestingQuestions.cs` at 29, total 269).
Each `.NET` file holds one category; the multi-category files (`SqlQuestions.cs`, `FrontEndQuestions.cs`,
`EngineeringPracticesQuestions.cs`) need their per-list split read by hand.

## Not tracked

The design tool's export archive (`.zip`), `uploads/` and `.thumbnail` are excluded — verified unused by
the render (the scene references no images), and the archive merely duplicates the extracted folder.

`master.mp4` and `out/` are gitignored too — they are regenerable render artifacts. The only committed
copies of the outputs are the ones copied into `docs/media/` and `web/public/media/`.
