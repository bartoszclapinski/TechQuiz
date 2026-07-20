# Handoff: TechQuiz — „Momentum" redesign (Landing, Auth, Categories, Quiz, Result, Dashboard)

## Overview
This is a full visual redesign of **TechQuiz** — a .NET learning app with multiple-choice
quizzes, AI-generated questions, spaced repetition, and gamified progress (XP, streaks,
"Skill IQ"). The redesign, codenamed **Momentum**, is warm and motivating: it pairs the
existing violet brand with an amber accent used specifically for gamification signals
(XP, streaks). It ships **light and dark themes** and six screens covering the core product
loop: Landing → Login → Categories → Quiz → Result → Dashboard.

The design goal that drove every layout decision: **look good on large / high-resolution
(UHD/4K) screens** while scaling cleanly down to mobile. This is achieved with centered
max-width containers, fluid `clamp()` typography and spacing, and `auto-fit / minmax`
grids — never fixed pixel columns.

## About the Design Files
The single file in this bundle — `TechQuiz Momentum.dc.html` — is a **design reference
created in HTML**. It is an interactive prototype demonstrating the intended look, themes,
typography scale, and interaction states. **It is not production code to copy directly.**

The file uses a small in-house component runtime (a `<x-dc>` template + a `Component`
logic class, loaded via `support.js`). **Ignore that runtime.** It exists only so the
prototype runs in our preview environment. Your job is to **recreate these designs in
TechQuiz's real codebase** using its established patterns and libraries.

Per the project's own README, the real web client is **React + Vite + TypeScript +
Tailwind CSS** (with shadcn/ui-style components). Rebuild the screens as React components
styled with Tailwind, mapping the tokens below to `tailwind.config` theme values and a
CSS-variable-based light/dark theme. If you open the file in a browser you can click the
top switcher to view every screen and toggle the theme — use that as your source of truth
for behavior.

## Fidelity
**High-fidelity (hifi).** Colors, typography, spacing, radii, shadows and interaction
states are all final and intentional. Recreate the UI faithfully. The exact hex values,
font sizes and the fluid `clamp()` ranges are all listed under **Design Tokens** — use
them verbatim.

---

## Design Tokens

### Fonts (Google Fonts)
- **Bricolage Grotesque** — headings / display / all big numbers. Weights 500–800.
  Almost always weight **800** for h1/display and stats, **700** for card titles (h3),
  **600** for smaller headings. Negative letter-spacing (see scale).
- **Geist** — body copy, buttons, labels, nav. Weights 300–700. Body is **400**,
  emphasised body/buttons **500–600**.
- **JetBrains Mono** — numeric/metadata labels, percentages, code answers, uppercase
  micro-labels, timers, "3 / 10" counters. Weights 400–700.

### Type scale (fluid — use these clamp() values)
| Role | Font | Size | Weight | Letter-spacing | Line-height |
|---|---|---|---|---|---|
| Hero display (Landing h1) | Bricolage | `clamp(44px, 6vw, 80px)` | 800 | -0.025em | 1.0 |
| Page h1 (Dashboard/Categories/Result) | Bricolage | `clamp(28px, 3vw, 40–44px)` | 800 | -0.02em | 1.05–1.1 |
| Section h2 | Bricolage | `clamp(28px, 3.2vw, 46px)` | 800 | -0.02em | 1.08–1.1 |
| Quiz question | Bricolage | `clamp(24px, 2.6vw, 34px)` | 800 | -0.01em | 1.25 |
| Card title (h3) | Bricolage | 19–21px | 700 | — | 1.2 |
| Big stat number | Bricolage | `clamp(38px, 4vw, 84px)` per context | 800 | -0.02/-0.03em | 0.9–1 |
| Body large (hero/subhead) | Geist | `clamp(16px, 1.4vw, 21px)` | 400 | — | 1.6 |
| Body | Geist | 15–16px | 400 | — | 1.5–1.6 |
| Small / caption | Geist | 13–14px | 400–500 | — | 1.5 |
| Micro-label (uppercase mono) | JetBrains Mono | 11px | 600 | 0.10–0.14em | — |
| Answer code / metadata | JetBrains Mono | 14–16px | 400–500 | — | — |

Base body size is **16px**. Never below **13px** anywhere.

### Colors

**Dark theme (default)**
| Token | Hex | Use |
|---|---|---|
| `--bg` | `#17121f` | page background |
| `--surface` | `#221a2e` | cards, header |
| `--elevated` | `#2a2137` | inputs, inner tiles, icon chips |
| `--border` | `#2a2137` | default borders |
| `--border2` | `#392c4d` | stronger borders, dashed "coming soon" |
| `--text` | `#f4f0fa` | primary text |
| `--text2` | `#a99fc0` | secondary text |
| `--text3` | `#7d7291` | muted / captions |
| `--accent` | `#a78bfa` | links, violet accents |
| `--track` | `#17121f` | progress-bar track |

**Light theme**
| Token | Hex | Use |
|---|---|---|
| `--bg` | `#faf8ff` | page background |
| `--surface` | `#ffffff` | cards, header |
| `--elevated` | `#f5f1fd` | inputs, inner tiles |
| `--border` | `#ece7f7` | default borders |
| `--border2` | `#ddd4ee` | stronger borders |
| `--text` | `#241b39` | primary text |
| `--text2` | `#6b6280` | secondary text |
| `--text3` | `#9a8fb5` | muted / captions |
| `--accent` | `#7c3aed` | links, violet accents |
| `--track` | `#f1ecfb` | progress-bar track |

**Brand / accent (both themes)**
| Token | Dark | Light | Use |
|---|---|---|---|
| `--brand` (gradient) | `linear-gradient(135deg,#a78bfa,#fbbf24)` | `linear-gradient(135deg,#7c3aed,#f59e0b)` | logo mark, avatars, progress fills, icon tiles |
| `--btn` (gradient) | `linear-gradient(135deg,#8b5cf6,#a78bfa)` | `linear-gradient(135deg,#7c3aed,#9333ea)` | primary buttons |
| `--brandfg` | `#17121f` | `#ffffff` | text/icon on top of `--brand` |
| `--ambertext` | `#fbbf24` | `#d97706` | XP / streak / % values |
| `--amberbg` | `rgba(251,191,36,.15)` | `#fef3c7` | XP / streak pill backgrounds |
| `--cardgrad` | `linear-gradient(150deg,#2a2137,#221a2e)` | `linear-gradient(150deg,#ffffff,#faf8ff)` | hero/feature spotlight cards |
| `--shadow` | `0 24px 60px rgba(0,0,0,.45)` | `0 24px 60px rgba(124,58,237,.14)` | floating cards |
| `--focusring` | `rgba(167,139,250,.22)` | `rgba(124,58,237,.16)` | input/selection focus ring |

**Semantic (quiz answer states, theme-independent)**
- Correct: border/badge `#22c55e`, fill `rgba(34,197,94,.12)`, check mark `#22c55e` (also `#4ade80` for positive deltas like "+6 Skill IQ").
- Wrong: border/badge `#ef4444`, fill `rgba(239,68,68,.12)`, cross mark `#ef4444`.
- Un-selected options after checking dim to `opacity: 0.5`.

**Ambient glows** (radial gradients behind heroes; purely decorative, low alpha):
`--heroglow1` amber ~`rgba(251,191,36,.12–.16)`, `--heroglow2` violet ~`rgba(167,139,250,.13–.18)`.

### Radius scale
- Pills / buttons / chips / avatars: `999px`
- Large cards / panels: `20–28px`
- Medium cards / inputs / answer options: `14–16px`
- Icon tiles / small chips: `9–15px`

### Spacing / layout
- Content max-width: **1560px**, centered (`margin: 0 auto`). Quiz column max **660px**;
  Login form column max **400px**; Result column max **560px**.
- Section padding is fluid: horizontal `clamp(20–24px, 3–4vw, 48–72px)`,
  vertical `clamp(28px, 3–7vw, 48–120px)`.
- Card grids: `grid-template-columns: repeat(auto-fit, minmax(<min>, 1fr))` with
  `gap: clamp(14px, 1.4vw, 24px)`. Min track values: dashboard bento `200px` (with the
  Skill IQ card spanning 2), category/detail cards `300–320px`, feature cards `280px`,
  landing hero split `400px`.

---

## Screens / Views

Global chrome (prototype only): a sticky top bar with screen tabs + a theme toggle.
**Do not build this bar** — it's a prototype navigator. In the real app, routing handles
screen changes and the theme toggle lives in app settings / header.

### 1. Landing
- **Purpose**: marketing entry; convert visitor to sign-up / demo.
- **Layout** (top→bottom, all inside the 1560px container):
  1. **Header**: logo (36px gradient rounded square with "T" + wordmark "TechQuiz"),
     nav (Features / Categories / How it works), right side "Sign in" link + "Get started"
     pill button.
  2. **Hero**: 2-col `auto-fit minmax(400px,1fr)` grid, gap `clamp(40px,5vw,88px)`.
     Left: amber pill "🔥 Keep your streak going", h1 "Level up your .NET skills, one quiz
     a day." (3 lines), body paragraph, two buttons ("Start earning XP" primary,
     "Try the demo" secondary), then a row of 3 stats (269 questions / 9 categories /
     100% free forever). Right: a floating "Daily goal" card (animated gentle float,
     `@keyframes` translateY ±8px over 8s) showing C# Advanced 87% progress + a 3-tile
     mini-stat row (12 day streak / 182 Skill IQ / 84% accuracy).
  3. **Topics chip row**: label + violet-active "C#" chip + neutral chips + "+2 more".
  4. **Features**: h2 + subhead + 3-card `auto-fit minmax(280px,1fr)` grid (✨ AI-generated
     questions / 🔁 Spaced repetition / 📈 Progress that motivates).
  5. **How it works CTA**: `--cardgrad` spotlight panel, 2-col; left h2 "Ready when you are."
     + button, right a numbered 1-2-3 list (Pick a category / Answer & learn / Track your climb).
  6. **Footer**: logo + "© 2026 TechQuiz · v0.1.0".

### 2. Login
- **Purpose**: sign in / enter demo.
- **Layout**: 2-col `auto-fit minmax(400px,1fr)`, min-height `calc(100vh - 57px)`.
  - **Left column**: logo top-left; form **vertically centered** in remaining space
    (`flex:1; align-items:center; justify-content:center`); "© 2026…" pinned bottom.
    Form (max 400px): h1 "Welcome back! 👋", subhead, Email input (prefilled
    `bartosz@example.com`), Password row with "Forgot?" link + focused password input
    (accent border + `--focusring` shadow, value `••••••••••`), primary "Sign in", secondary
    "Continue as demo", footer line "Don't have an account? Create one".
  - **Right column** (`--elevated` bg with two ambient glows): a filled marketing panel —
    mono kicker "SHARPEN YOUR SKILLS", h2 "AI-generated quizzes that adapt to how you learn.",
    a paragraph, a 3-tile stat row (🔥 12 day streak / 182 Skill IQ / 84% accuracy), a
    testimonial card (quote + avatar "MK · Marta K. · Backend developer"), and a social-proof
    row (overlapping avatar circles + "Join 1,200+ developers leveling up"). Floating cards
    animate gently.
- Inputs: full-width, padding `15px 17px`, radius `14px`, 16px Geist, bg `--elevated`,
  border `--border`; focused = border `--accent` + `box-shadow: 0 0 0 3px var(--focusring)`.

### 3. Categories
- **Purpose**: choose a quiz topic.
- **Layout**: app header (logo, nav with "Categories" active, avatar "BC"), then h1
  "Pick a category" + subhead, then a card grid `auto-fit minmax(300px,1fr)`.
  - **Active cards** (4: C# Basics 87% / ASP.NET Core 72% / EF Core 94% / SQL 65%): gradient
    icon tile (monogram), question-count pill (e.g. "18 q"), title, one-line description, and
    a progress bar + % in amber. Whole card is clickable → Quiz. `cursor:pointer`.
  - **"Coming soon" cards** (5: Design Patterns / Unit Testing / Git / Clean Architecture /
    REST API): dashed `--border2` border, `opacity:.7`, muted icon tile, "Soon" pill,
    "Not started" label. Non-interactive.

### 4. Quiz (interactive)
- **Purpose**: answer one multiple-choice question with immediate feedback.
- **Layout**: min-height `calc(100vh - 57px)`, column.
  - **Top progress bar** (max 900px): "C# Basics · 3 / 10" + progress track (30%) + amber
    "+40 XP" pill + a ✕ exit button (→ Categories).
  - **Center** (max 660px): difficulty pill ("Medium"), the question (Bricolage 800), then a
    vertical list of 4 answer options.
  - **Answer option**: full-width button, `16px 20px` padding, radius `16px`, 1px border.
    Left = numbered mono badge (1–4), center = the answer in JetBrains Mono, right = a
    result mark slot. Content: `private` / `internal` / `protected` / `public`;
    **correct = `internal` (index 2)**.
  - **Footer row**: hint "Press 1-4 to select" + a primary button. Before checking:
    "Check answer" (disabled until an option is selected). After checking: "See results →"
    (→ Result).
- **States** (all in the prototype):
  - *Idle*: neutral options.
  - *Selected* (pre-check): selected option gets accent border + `--focusring` ring +
    gradient badge.
  - *Checked*: correct option → green border/fill/badge + "✓"; if the user picked wrong,
    their choice → red border/fill/badge + "✗"; all other options dim to `opacity .5`;
    a green explanation panel appears below ("Correct — internal" + why). Options become
    non-interactive (`cursor:default`).

### 5. Result
- **Purpose**: celebrate completion, show rewards, route onward.
- **Layout**: centered column (max 560px) over ambient glows. Top: amber pill "🔥 Streak
  saved · 13 days". Center: a large circular score badge (`clamp(150px,20vw,190px)`, surface
  bg, big "87%" + "13 / 15 correct"). Then h1 "Quiz complete! 🎉", subhead. A 3-tile reward
  row (+180 XP amber / 4:12 time / +6 Skill IQ green). Two buttons: "Review answers"
  (secondary → back to Quiz) and "Back to dashboard" (primary → Dashboard).

### 6. Dashboard
- **Purpose**: home base; progress overview.
- **Layout**: app header (nav "Dashboard" active), then a header row (h1 "Nice work today 👏"
  + subhead on the left, "Start today's review · 8 due" primary button on the right, wraps on
  narrow). Then a **bento grid** in two grid blocks:
  - Top block `auto-fit minmax(200px,1fr)`: **Skill IQ** hero card **spanning 2 columns**
    (`--cardgrad`, ambient glow, mono "SKILL IQ", huge `clamp(56px,6vw,84px)` "182", "▲ 14
    this week" green, "Top 18%…", and a Level-7 XP bar 640/800). Plus **Streak 🔥** card
    ("12 days", "Best: 21 days") and **Accuracy** card ("84%", "418 / 497 correct").
  - Bottom block `auto-fit minmax(320px,1fr)`: **Category progress** card (4 rows: icon tile +
    name + % + bar), **Weekly activity** card (7 vertical bars M–S; active days use the brand
    gradient, rest `--elevated`), **Recent attempts** card (3 rows: icon + name + timestamp +
    colored % — green for high, amber for low).

---

## Interactions & Behavior
- **Navigation**: Landing "Get started"/"Sign in" → Login. Login "Sign in"/"Continue as
  demo"/"Create one" → Dashboard. Categories active card → Quiz. Quiz ✕ → Categories,
  "See results →" → Result. Result "Back to dashboard" → Dashboard, "Review answers" → Quiz.
  Logo → Landing. (In the prototype these are click handlers; in the app use the router.)
- **Quiz answer flow** (see Screen 4 states). Selecting is blocked once checked. "Check
  answer" is only enabled when an option is selected. The prototype hard-codes the correct
  index as 2 (`internal`) and a static explanation — in the real app this comes from the
  question model.
- **Theme**: light/dark via CSS variables on a root wrapper. Prototype persists choice to
  `localStorage['tq_theme']` and last screen to `localStorage['tq_screen']`; transition
  `background .25s, color .25s`. In the app, use your theme provider.
- **Animations**: floating cards use a subtle `translateY` keyframe loop (±5–8px, 6.5–8.5s,
  ease-in-out). Keep it gentle; respect `prefers-reduced-motion`.
- **Responsive**: everything is driven by `clamp()` + `auto-fit minmax()`, so columns
  collapse automatically. Confirm the bento, hero split, and login split all fold to a single
  column on mobile and that hit targets stay ≥44px.

## State Management
- `theme`: `'dark' | 'light'` — persisted.
- `screen` / route — persisted in prototype; real app uses router.
- Quiz: `selectedAnswer: number | null`, `checked: boolean`. Real app additionally needs the
  current question index, the question/answers/correctIndex/explanation from the API, running
  score, XP, timer, and streak — all currently mocked with static values.
- Data (mocked here, fetch in real app): category list with progress %, dashboard stats
  (Skill IQ, streak, accuracy, weekly activity, recent attempts), user profile.

## Assets
- **Fonts**: Bricolage Grotesque, Geist, JetBrains Mono — Google Fonts.
- **Icons**: none as files. Emoji are used intentionally as lightweight iconography
  (🔥 streak, ✨🔁📈 features, 🎉👏👋). Category "icons" are **text monograms** (C#, ASP, EF,
  SQL…) on gradient tiles. If your codebase has an icon set (e.g. Lucide), you may swap the
  monograms/emoji for real icons — keep the gradient tile treatment.
- **Images**: none. Avatars are gradient circles / initials placeholders.
- No proprietary brand assets.

## Files
- `TechQuiz Momentum.dc.html` — the interactive design reference. Open in a browser, use the
  top switcher to view all six screens and toggle light/dark. Treat its rendered output +
  this README as the spec; treat its `<x-dc>`/`Component`/`support.js` runtime as throwaway
  scaffolding, not an implementation pattern.
