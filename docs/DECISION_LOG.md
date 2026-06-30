# Decision Log

This document records the major architectural and project decisions made during TechQuiz development. Each entry follows a lightweight Architecture Decision Record (ADR) format: context, decision, consequences. Entries are append-only — superseded decisions are marked but not deleted.

---

## ADR-001: Project Naming

**Status:** Accepted
**Date:** 2026-05-11

### Context
The project needed a name. Several candidates were explored across multiple categories: descriptive (TechQuiz, QuizDev), wordplay (Sharpen, Recall), space-themed (Apogee, Parsec, Astrolab, Velara), and abstract neologisms (Knowel, Cogwise, Querion, Mentlo). Each candidate was checked for trademark conflicts and product collisions in similar domains.

### Decision
The project is named **TechQuiz**. It is descriptive, unambiguous, and free from confusion with existing commercial products in the same problem space.

### Consequences
- Repository: `github.com/bartoszclapinski/techquiz`
- Namespaces: `TechQuiz.Domain`, `TechQuiz.Application`, `TechQuiz.Infrastructure`, `TechQuiz.API`
- The name is straightforward — rename is possible later via solution-wide find/replace if the project evolves into a commercial product.

### Alternatives Rejected
All evaluated alternatives had significant collisions in the AI/learning/coding space, including Knowely (170k+ users, identical use case), SkillForge (multiple competing platforms), Querion (math solver app), and Velara (goal tracker app with similar gamification approach). Time spent on naming was deemed disproportionate to value for a portfolio project.

---

## ADR-002: English-only Project Language

**Status:** Accepted
**Date:** 2026-05-11

### Context
The project owner is a Polish developer targeting both Polish and international software houses. Project artifacts (code, UI, commits, documentation, issue tracker) could be written in Polish or English.

### Decision
All public project artifacts are written in **English**: source code identifiers, comments, UI text, commit messages, documentation, issue tracker entries, and pull request descriptions. Private planning documents may remain in Polish.

### Consequences
- Reduces friction for international recruiters
- Demonstrates technical English competency without separate certification
- Practices industry-standard project conventions
- Polish working notes are kept outside the public repository

---

## ADR-003: Multi-User Architecture from Day One

**Status:** Accepted
**Date:** 2026-05-11

### Context
The application could be designed as single-user (personal app, no auth) or multi-user (with authentication and per-user data). Multi-user adds complexity but signals production-grade thinking.

### Decision
The application is **multi-user from day one** using ASP.NET Core Identity and JWT bearer tokens. Initial implementation includes only registration and login — password reset, email confirmation, refresh tokens, and roles are deferred to Phase 4.

### Consequences
- All entities with user-scoped data (`QuizAttempt`, `UserProgress`, etc.) include `UserId`
- All API endpoints except `/api/auth/*` require authorization
- Frontend implements protected routes, JWT storage, and token attachment
- MVP scope increased by approximately one development week

---

## ADR-004: React + TypeScript for Frontend

**Status:** Accepted
**Date:** 2026-05-11

### Context
The frontend could be built with React + TypeScript, Blazor WebAssembly, or Blazor Server. The developer's primary stack is .NET, which would make Blazor the path of least resistance, but React + TS is the dominant frontend technology in commercial job postings.

### Decision
The frontend is built with **React 18 + TypeScript** using Vite as the build tool.

### Consequences
- Demonstrates full-stack capability beyond .NET ecosystem
- Aligns with the majority of .NET job postings that list React/Angular/Vue as nice-to-have or required
- Existing portfolio already includes Blazor (DevMetricsPRO) — React adds a new skill rather than duplicating
- Monaco Editor (for Phase 3 code questions) integrates more naturally with React than Blazor
- Increases learning curve for the developer; mitigated by just-in-time learning during implementation

### Alternatives Rejected
Blazor WASM was considered for its single-language stack benefit but deemed redundant given existing portfolio coverage.

---

## ADR-005: PostgreSQL over Firebase

**Status:** Accepted
**Date:** 2026-05-11

### Context
Firebase was briefly considered for its turnkey hosting, integrated auth, and zero infrastructure setup. PostgreSQL requires Docker setup but provides relational data modeling and standard .NET ecosystem alignment.

### Decision
The primary data store is **PostgreSQL 16** accessed via Entity Framework Core 9. Local development runs PostgreSQL in Docker via docker-compose.

### Consequences
- Consistent with .NET ecosystem expectations from recruiters
- Relational model naturally fits Quiz → Questions → Answers and QuizAttempt → Responses
- EF Core handles relationships, migrations, and LINQ-to-SQL translation
- Production deployment will likely use Railway, Render, or Azure Database for PostgreSQL

### Alternatives Rejected
Firebase's Firestore (NoSQL) would require manual denormalization and join logic, undermining the relational nature of the domain. Firebase Auth would replace ASP.NET Core Identity, eliminating a portfolio-relevant skill demonstration.

---

## ADR-006: Multi-Provider AI with User-Supplied Keys

**Status:** Accepted — **Amended by ADR-019** (provider set expanded to native OpenAI/Anthropic/Gemini + OpenRouter; voice funding model added)
**Date:** 2026-05-11

### Context
AI integration is a core feature. The system could lock to a single provider (OpenAI or Anthropic), or support multiple providers with per-user configuration. Cost ownership also needed resolution: the project owner could pay for all generation, or users could supply their own keys.

### Decision
The AI layer uses a **provider abstraction** (`IAiProvider`) with concrete implementations for OpenAI and Anthropic Claude. Users supply their own API keys via the application UI. Keys are encrypted at rest using ASP.NET Core's `IDataProtectionProvider`.

### Consequences
- No AI cost burden on the project owner
- Users choose their preferred provider based on personal API access
- New providers (Gemini, local LLMs) can be added by implementing the interface
- Security responsibility for key encryption increases — keys must never be logged, must be decrypted only at request time, and must use platform-recommended encryption
- Per-user rate limiting required to prevent accidental cost spikes

---

## ADR-007: Public AI-Generated Question Pool

**Status:** Accepted
**Date:** 2026-05-11

### Context
When a user generates a question using their API key, the generated content could remain private to that user (private pool) or be shared with all users (public pool). Public pool amortizes costs but raises moderation concerns.

### Decision
AI-generated questions are saved to a **public pool** accessible to all users. Generation metadata is preserved (generating user, provider, generation timestamp). Quality is controlled via automatic validation (schema checks) and community moderation (voting).

### Consequences
- First user to generate a question on a topic effectively pays for everyone
- Question bank grows organically with usage
- Quality control system must be implemented from Phase 3 onward
- Moderation queue and community voting need UI in Phase 3 or 4
- A moderation policy must be documented (what constitutes acceptable content)

### Alternatives Rejected
Private pool was considered but rejected because it fails to leverage the network effect — most generated questions would be duplicates across users, wasting API calls.

---

## ADR-008: TDD for Domain and Application Layers

**Status:** Accepted
**Date:** 2026-05-11

### Context
Test-driven development could be applied to the entire codebase or selectively. Full-stack TDD slows development significantly, especially for UI and infrastructure code. Selective TDD focuses effort where business logic correctness matters most.

### Decision
**TDD is applied to the Domain and Application layers only.** Tests are written before implementation for entities, value objects, domain services, and CQRS handlers. Infrastructure code and controllers receive tests after implementation, primarily through integration tests in Phase 4. Frontend components receive tests in Phase 4 via Vitest and React Testing Library.

### Consequences
- Core business logic (scoring, spaced repetition, quiz state transitions) has high test coverage and confidence
- Refactoring of business logic is safe and frequent
- Infrastructure changes (EF Core configurations, AI provider implementations) are verified through integration tests rather than per-unit tests
- Initial development velocity is reduced compared to no-TDD, but later refactoring is significantly faster

---

## ADR-009: Conventional Commits with commitlint

**Status:** Accepted
**Date:** 2026-05-11

### Context
Commit message conventions could be informal, semi-structured, or strictly enforced. Conventional Commits is an industry-standard format with tooling support.

### Decision
All commits follow the **Conventional Commits** specification, enforced by **commitlint** via a husky `commit-msg` hook. Scope is omitted (e.g., `feat: add JWT auth` rather than `feat(auth): add JWT auth`) to reduce overhead in a monolithic project.

Allowed types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`, `perf`, `ci`, `build`, `revert`.

### Consequences
- Git history is consistently readable and filterable
- Squash merges produce clean commits on `master`
- Enables future automation (changelog generation, semantic versioning) if needed
- Adds a small setup step in Phase 0 (npm init, commitlint install, husky setup)
- Demonstrates production-grade discipline to recruiters

---

## ADR-010: GitHub Flow with Squash Merge

**Status:** Accepted
**Date:** 2026-05-11

### Context
Branching strategies range from trunk-based (commit directly to `master`) to Git Flow (multiple long-lived branches). For a solo project, the question is whether the overhead of a feature-branch workflow is worth the discipline and portfolio signal.

### Decision
The project uses **GitHub Flow**: feature branches, pull requests with self-review, and squash merge to `master`. Branch protection rules on `master` enforce the workflow.

Branch naming follows the same vocabulary as commit types: `feature/short-description`, `fix/short-description`, `refactor/short-description`, etc.

### Consequences
- Every feature has a self-review checkpoint before merging
- `master` history stays clean with one commit per feature
- CI gates merges (tests must pass)
- Demonstrates standard commercial workflow to recruiters
- Adds minor overhead per change; mitigated by branch naming automation

---

## ADR-011: Console + Seq Logging

**Status:** Accepted
**Date:** 2026-05-11

### Context
Logging options ranged from minimal (console only) to enterprise (full ELK stack, Datadog). For a portfolio project, the goal is demonstrable observability without infrastructure complexity.

### Decision
Logging uses **Serilog** with two sinks: **Console** for development readability and **Seq** for structured log search. Seq runs in a Docker container via docker-compose alongside PostgreSQL.

All log statements use structured properties (`"User {UserId} did {Action}"`) rather than string interpolation, enabling rich filtering in Seq.

### Consequences
- Observability is searchable and filterable from day one
- Recruiters see modern logging practices in the project
- One additional container in local development (~200MB RAM)
- Trains the developer in structured logging habits
- Production deployment may swap Seq for hosted equivalent (Seq Cloud, Datadog, or similar)

---

## ADR-012: Premium SaaS Aesthetic with Dual Theme

**Status:** Accepted
**Date:** 2026-05-11

### Context
UI direction could range from corporate/minimal to playful/gamified to terminal/retro. The choice signals project personality and affects implementation complexity. Single-theme vs dual-theme implementation also needed resolution — dual-theme requires design tokens from day one but is significantly more impressive in portfolio.

### Decision
The visual design follows a **premium SaaS aesthetic** in the spirit of Linear, Vercel, and Stripe. The application supports **both dark and light themes** with a user-controlled toggle.

**Default theme:** dark mode (slate-950 background, slate-900 surfaces)
**Alternative theme:** light mode (white background, slate-50 surfaces)
**Accent color:** violet-500 in dark mode, violet-600 in light mode (slightly darker for contrast)

All UI components consume design tokens via CSS variables, never hardcoded colors. This enables theme switching with a single class toggle on `<html>`.

Typography uses **Geist** for UI and **JetBrains Mono** for code. Motion is restrained — short transitions, subtle shimmer for loading, no flashy animations.

Phase 2 introduces a **bento grid layout** for the progress dashboard. Theme toggle is implemented in Phase 1 MVP using a placeholder UI element, with full polish (icon transitions, system preference detection) deferred to Phase 4.

### Consequences
- Aligns visually with modern, recognizable SaaS products
- Dual theme implementation requires design tokens from day one — slightly more discipline upfront but no refactor cost later
- Portfolio differentiation: most portfolio projects ship single-theme; dual theme demonstrates production-grade theming
- Code syntax highlighting (Monaco Editor, code questions) requires two themes: VS Code Dark+ for dark mode, GitHub Light for light mode
- Theme preference persisted in localStorage with system preference (`prefers-color-scheme`) as fallback
- Light mode rendering of code blocks requires extra attention to maintain readability

---

## ADR-013: Scope Strategy — MVP First, Iterate

**Status:** Accepted
**Date:** 2026-05-11

### Context
Full scope (multi-user + AI + TDD + React + dashboard + gamification) realistically required 5-6 months. The project owner needed a faster path to a showable portfolio entry.

### Decision
Development follows an **MVP-first strategy** with four phases:

- **Phase 0** — Project setup (auth, structure, CI, Docker)
- **Phase 1** — MVP: quiz flow end-to-end with hardcoded seed questions, no AI yet
- **Phase 2** — History, dashboard with bento grid, spaced repetition
- **Phase 3** — AI generator, multi-provider abstraction, code questions
- **Phase 4** — Gamification, deployment, polish

Each phase produces a showable, demonstrable state. The MVP (end of Phase 1) is intentionally shippable to portfolio.

### Consequences
- Earlier portfolio milestone (MVP shippable after Phase 1)
- AI integration deferred to Phase 3 — the "wow" factor arrives later but on a stable foundation
- Each phase milestone is an opportunity to pause, evaluate, and adjust priorities
- Avoids the "perfect-but-never-shipped" anti-pattern common in solo projects

---

## ADR-014: Application Shell — Topbar Navigation

**Status:** Accepted
**Date:** 2026-05-11

### Context
The application shell could use either a sidebar (vertical left nav) or topbar (horizontal top nav) for primary navigation. Sidebar scales better as features grow and provides more space for user context (avatar, level, settings). Topbar is more familiar from classic web applications and uses horizontal space more efficiently for content.

### Decision
The application uses a **topbar navigation** layout. The topbar contains: logo (left), primary nav items (center, horizontal), and user controls (right — theme toggle, avatar). Quiz mode hides the topbar entirely for a focused, distraction-free experience.

Phase 2 and 3 features (Daily Review, Generate, Dashboard, History) appear in the topbar with `soon` badges and disabled state until implemented.

### Consequences
- Familiar layout for users of classic web applications
- Content gets full horizontal width (3-column category grid instead of 2-column with sidebar)
- User info compressed to avatar only — email and level not visible in shell, but accessible via avatar dropdown or profile page
- If nav items grow beyond ~6, topbar may require overflow handling (dropdown menu or horizontal scroll)
- Quiz full-screen mode requires explicitly hiding the topbar — implemented by route-aware shell component

### Alternatives Rejected
Sidebar was considered for better scalability and more space for user context. Topbar was chosen for familiarity and content-first horizontal layout.

---

## ADR-015: Quiz UI Patterns

**Status:** Accepted
**Date:** 2026-05-11

### Context
The quiz screen is the central experience of the application. Several decisions needed coordination: how to present questions, how users select answers, how progress is shown, and how the experience differs from the rest of the app shell.

### Decision

**Layout.** Quiz runs in full-screen mode — the application topbar is hidden by the route-aware shell component, replaced with a minimal quiz header (category name, question counter, progress bar, exit button). Question content is centered with a maximum width of 600px for focus and readability.

**Question types in MVP.** Only Multiple Choice questions ship in MVP. Code questions (output prediction, bug finding, fill-in) ship in Phase 3 with Monaco Editor integration and AI evaluation.

**Answer selection.** Each option is presented with a keyboard shortcut prefix (1, 2, 3, 4) in JetBrains Mono in a small rounded square. This communicates that the quiz is keyboard-friendly. Selected state combines three visual changes: violet border, violet-filled prefix square, and a 3px violet outer glow ring. A persistent keyboard hint at the bottom shows `Press 1-4 to select, Enter to continue`.

**Test mode flow.** Users see no immediate feedback after answering — correct/incorrect reveals come only on the Result screen. Users can change their answer freely before clicking Next, supporting reflection over reaction.

**Progress indicator.** A thin 3px progress bar in the header, filled in violet, animated with `transition: width 0.3s` between questions. Combined with a textual counter (`C# Basics · 3 of 10`) in monospace, giving both peripheral awareness and exact position.

**Question metadata.** Each question shows only a difficulty badge (Easy/Medium/Hard) using semantic colors (emerald/amber/red) in 10-15% opacity backgrounds. AI-generated badges and topic tags are deferred to Phase 3 to keep the MVP layout clean.

### Consequences
- Quiz is keyboard-driven by design, appealing to power users
- Test mode supports honest self-assessment without the bias of immediate feedback
- Code question forward-look mockups exist for Phase 3 reference (border-left violet accent, dark code background in both themes)
- Full-screen mode requires the shell to be route-aware — `/quiz/:id` route hides the topbar entirely
- Mobile responsive layout (Phase 4) will need to adapt option buttons to stack vertically with larger tap targets

---

## ADR-016: Dashboard Bento Grid Layout

**Status:** Accepted
**Date:** 2026-05-11
**Implementation phase:** Phase 2

### Context
The progress dashboard is the most data-rich screen in the application. It needs to show multiple types of information (stats, charts, activity feed, recommendations) without overwhelming the user. A uniform grid would feel monotonous; varied tile sizes create visual hierarchy and rhythm.

### Decision
The dashboard uses a **3-column bento grid** on desktop. Tiles have varying sizes via CSS Grid `grid-column: span N` and `grid-row: span N` properties.

**Tile inventory (Phase 2):**

1. **Current streak** (1x1) — large day count with flame icon and 14-day sparkline
2. **Score over time** (2x1) — line chart with gradient fill, trend badge, dashed grid lines, last point highlighted
3. **Category strength radar** (1x2) — 5-axis radar with semi-transparent violet fill and solid outline
4. **Questions answered** (1x1) — count with monthly delta
5. **Average score** (1x1) — percentage with trend chevron
6. **Recent activity** (2x1) — list of 5 most recent attempts with category, time, duration, score (color-coded)
7. **Best category** (1x1) — emerald-accented success tile
8. **Needs practice** (1x1) — amber-accented tile with primary CTA button to start practicing

**Chart rendering.**
- Line charts use smooth Bezier curves (no step-style) with violet gradient fills underneath (35% opacity at top to 0% at bottom in dark; 22% to 0% in light)
- Radar charts use translucent violet fill (25% dark / 18% light) with solid outline at full violet
- Sparklines are inline SVG, no axes, no labels, pure shape

**Header.** "Welcome back, [Name]" with a contextual subtitle showing last attempt info. A segmented control (Week / Month / All time) sits in the top-right corner, defaulting to Month.

**Empty state.** First-time users see a dashboard with disabled placeholder tiles (opacity 0.4, em-dash `—` for missing values) and a hero CTA card in the score-chart slot inviting them to take their first quiz. This preserves the dashboard structure as a preview of what will populate after the first attempt.

### Consequences
- Most visually impressive screen in the portfolio — direct comparable to Linear, Vercel, Stripe dashboards
- Recharts library (already in stack) handles line and radar charts; sparklines implemented as inline SVG paths
- Implementation effort for Phase 2 is significant — about half of Phase 2 work is the dashboard
- Empty state preserves user orientation by showing what data will appear
- Mobile responsive layout (Phase 4) will collapse 3 columns to 1, requiring tile reordering for narrative flow on small screens

---

## ADR-017: CI/CD Strategy

**Status:** Accepted
**Date:** 2026-05-11

### Context
The project needs continuous integration on PRs and continuous deployment to a publicly accessible environment. Several decisions had to coordinate: branch protection rigor, mandatory checks, deploy target, versioning, and timing of when deployment becomes part of the workflow.

### Decision

**Branch protection.** `master` is protected. All changes go through pull requests. CI must pass before merge. Review approvals are not required (solo project). Squash-merge only, linear history enforced. Force pushes and deletions blocked.

**Required CI checks.** Three jobs run on every PR: backend (`dotnet build` + `dotnet test` against ephemeral PostgreSQL service), frontend (`pnpm lint` + `pnpm build`), and commitlint (PR title + every commit in PR validated against Conventional Commits).

**Semantic-release.** Automated versioning and CHANGELOG generation enabled from the start, not deferred. Triggered on every push to `master`. Commit types determine version bump (`feat:` → minor, `fix:` → patch, `BREAKING CHANGE` → major). Releases create Git tags and GitHub Releases automatically.

**Staging deployment.** Azure App Service (Linux, .NET 9 runtime) chosen as deploy target instead of Fly.io, Railway, or Render. The rationale is portfolio fit: this project targets .NET developer roles, and Azure familiarity is a directly relevant signal for those positions. Staging deploys are scheduled as iteration 1.8, after Phase 1 (MVP) feature work completes.

**Production deployment.** Deferred to Phase 4. The same Azure pipeline pattern will be reused; a separate `production` environment in GitHub will require manual approval and pin to git tags.

**Secret management.** GitHub Secrets at the repository level for shared values (Azure credentials). GitHub Environments (`staging`, eventually `production`) for environment-specific overrides. Local development uses `dotnet user-secrets` — no secrets committed.

### Consequences
- Every commit on `master` represents a passing build — `master` is always deployable
- Squash-merge keeps history readable: one commit per feature on `master`, full granularity preserved in PR
- semantic-release produces real version numbers from day one, building a meaningful tag history for the portfolio
- Azure App Service signals .NET cloud familiarity to recruiters — directly relevant to most job postings in the niche
- App Service free tier has cold starts; portfolio demos may need to "wake up" the app — acceptable trade-off for $0 hosting cost
- Custom domain optional; staging URL `techquiz-*.azurewebsites.net` works as portfolio link without one
- Production deployment in Phase 4 reuses the same workflow shape, lowering the new-work cost when production rolls out

### Alternatives Rejected
- **Fly.io** — strong choice for solo Docker deploys, but doesn't signal .NET-on-cloud experience the way Azure does. Rejected for portfolio fit, not technical limitations.
- **Required code review** — typical in commercial teams but adds friction with no benefit in a solo project. CI is the gatekeeper instead.
- **Deferring semantic-release** — would have meant manual version bumps and CHANGELOG edits later. Setup cost upfront is small (~10 min); benefit (clean release history visible to recruiters) is large.
- **Skipping staging in MVP** — would have meant the portfolio piece is "code-only" until Phase 4. Live URL is a major credibility signal; worth the dedicated iteration.

---

## ADR-018: Online Code Execution for Coding Questions

**Status:** Accepted
**Date:** 2026-06-23

### Context
The roadmap (Phase 3, see ADR-013 and `sprint3/README.md`) originally scoped "code questions" as three non-executing variants — **CodeOutput** (predict the output), **CodeFix** (find the bug), and **FillIn** (complete the snippet) — graded either against a stored answer or by an AI evaluator. None of these run user code.

We now want a stronger, more differentiated capability: **coding tasks of varying difficulty where the user writes C# and the platform actually compiles and runs it against hidden tests**, reporting pass/fail like Exercism or LeetCode. C# is the only target language initially; more languages are a later, deliberate extension.

The hard problem is **executing untrusted code safely**. User submissions can attempt file/network access, infinite loops, fork bombs, and memory exhaustion. Running such code in the API process is not acceptable.

### Decision

**A new execution-backed question type is introduced** — `CodeChallenge` — alongside the previously planned non-executing variants. For `CodeChallenge`, the user writes a C# function/program; the platform compiles it, runs it against a hidden test harness in an isolated sandbox, and grades on observed behavior (stdout / exit code / test assertions), not on AI judgment.

**Execution runs on a self-hosted Judge0 instance**, not in-process and not in the API container. The API submits `{ source, stdin, expected output / test harness, language=C# }` to Judge0 over its REST API; Judge0 compiles and runs the code under its `isolate`/cgroups sandbox (no network, CPU/memory/wall-time limits, read-only filesystem) and returns stdout, stderr, exit status, and timing. Judge0 ships as additional services in `docker-compose` (its own server, workers, PostgreSQL, and Redis), keeping it isolated from the application database and process.

**Language scope.** C# only at launch. Judge0 supports ~60 languages out of the box, so adding languages later is configuration and content work, not an architectural change — this is the reason Judge0 is preferred over a bespoke .NET-only runner.

**AI's role is reframed, not removed.** AI evaluation (ADR-013 / sprint3) remains valuable for *qualitative feedback* on a submission ("you missed the null case"), but it is no longer the grader for executable tasks — the test harness is. AI feedback becomes complementary to deterministic pass/fail, not a substitute for it.

**This ADR supersedes the implicit Phase 3 decision** that all code questions are AI-evaluated and non-executing. The non-executing variants (CodeOutput, CodeFix, FillIn) still stand; `CodeChallenge` is additive and execution-backed.

### Consequences
- Coding tasks become genuinely interactive — write, run, see real test results — which is the most memorable differentiator in the portfolio.
- Untrusted code never touches the API process or the application database; the blast radius of a malicious submission is confined to an ephemeral, network-less, resource-capped Judge0 sandbox.
- The local stack grows by several containers (Judge0 server + workers + its own Postgres + Redis). `docker compose up` is heavier; the portable-deploy story (Docker on any host) still holds, just with a larger footprint.
- Difficulty levels map naturally onto challenge design: from "return the sum of two ints" (Easy) to "implement an algorithm handling edge cases and performance" (Hard), graded by the hidden test set.
- A dedicated iteration (and content authoring for test harnesses) is needed; this is net-new scope beyond the original Phase 3 outline and will get its own sprint detail.
- Operational surface increases: Judge0 versions, sandbox config, and resource limits become things to maintain and tune.

### Alternatives Rejected
- **Roslyn scripting in-process (`Microsoft.CodeAnalysis.CSharp.Scripting`)** — fastest to prototype and pure .NET, but runs arbitrary user code inside the API process with full access to the host. Safe use would require building a sandbox ourselves; getting that wrong on a public portfolio that runs strangers' code is an unacceptable liability.
- **Bespoke per-submission Docker containers** — strong isolation, but reimplements what Judge0 already provides (queueing, language toolchains, resource limits, result capture). Reinventing the sandbox is effort better spent on product.
- **Piston (engineer-man)** — lighter (single container, no DB/Redis) and a viable fallback, but a smaller language/runtime ecosystem and a less feature-complete API (no built-in queue/limits semantics) than Judge0. Kept as a contingency if Judge0's footprint proves too heavy for the target host.
- **Hosted Judge0 via RapidAPI** — removes self-hosting effort, but adds an external dependency, per-call rate limits, and cost, and sends user code off-box. Self-hosting keeps the system self-contained and free.

---

## ADR-019: AI Provider Set — Native OpenAI/Anthropic/Gemini plus OpenRouter

**Status:** Accepted
**Date:** 2026-06-25

**Amends ADR-006** (Multi-Provider AI with User-Supplied Keys). The provider abstraction, bring-your-own-key model, and `IDataProtectionProvider` key encryption from ADR-006 all stand unchanged. This ADR only revises *which* providers ship and adds the voice funding model.

### Context
ADR-006 scoped concrete providers as **OpenAI + Anthropic** and treated Gemini / local models as "addable later via the interface." When planning Phase 3, two questions forced a revision:

1. **How many native providers do we build, and do we even need them given OpenRouter?** OpenRouter is a single OpenAI-compatible endpoint that reaches Claude, GPT, Gemini, and open models on one key. The temptation was to ship only a native Anthropic client plus OpenRouter and call it "everything."
2. **Who pays for voice** in the planned interview-simulation feature (separate idea; ElevenLabs for TTS)?

The OpenRouter-only-plus-Anthropic line was rejected on **user-experience grounds**: bring-your-own-key exists to accept the key a user *already has*. Forcing someone who already funds an OpenAI (or Google) API key to create an OpenRouter account and load separate credit there is real friction that defeats the purpose. The provider abstraction makes each native client cheap, so minimizing integration count at the cost of UX is a bad trade.

### Decision
The AI layer ships **four `IAiProvider` implementations**:

- **Anthropic** (native) — first to be built and the only provider verifiable end-to-end today (the only key the owner funds during development).
- **OpenAI** (native) — so users already on OpenAI use their existing key.
- **Gemini** (native) — so users in the Google ecosystem use their existing key.
- **OpenRouter** — a single OpenAI-compatible client reaching many models on one key, offered as an *additional* convenience for users who deliberately want one key for everything. It does **not** replace the native providers.

No standalone provider is built per model; "provider" = an integration (key + HTTP client + parsing), "model" = what you reach through it. Four integrations, many reachable models.

**Build / verification order.** Anthropic is implemented and verified live first. OpenAI, Gemini, and OpenRouter are built behind the same seam and verified with mocked-HTTP integration tests; each is confirmed against the real API once a key for it is available. We do not claim a provider "works" before it has been run against its real endpoint.

**Funding model.** LLM usage stays fully bring-your-own-key (users pay, per ADR-006). For the future interview-simulation feature, the **owner funds the ElevenLabs voice** (TTS/STT) himself, while users still supply their own LLM keys. The app will later include per-provider tutorials walking users through creating an API key.

### Consequences
- Users onboard with whatever provider they already use — no forced signup or fund-transfer to a gateway. Lower adoption friction, which is the whole point of BYO-key.
- Four clients to maintain instead of two; mitigated by the shared `IAiProvider` seam, the resolver, and shared response-mapping. Adding a fifth provider stays cheap.
- OpenRouter's presence means breadth (any model it routes) is available without a native client per vendor, for users who opt into it.
- Verification is staggered: only Anthropic is provable live now; the others rely on mocked-HTTP tests until keys exist. This is an honest limitation, not a gap to paper over.
- `AiProviderKind` grows from `{ Anthropic, OpenRouter }` (shipped in iteration 3.1) to `{ Anthropic, OpenAi, Gemini, OpenRouter }`.
- Voice cost sits with the owner, bounded to the interview feature; LLM cost never does.

### Alternatives Rejected
- **Native Anthropic + OpenRouter only** — fewest integrations and OpenRouter technically reaches every model, but forces non-Anthropic users onto a new account with separately-funded credit. Rejected on UX: BYO-key must honor the key users already hold.
- **OpenRouter only (zero native clients)** — maximal simplicity, single integration, but every user must use OpenRouter regardless of what they already pay for. Same friction, more acutely.
- **Owner funds all LLM generation** — removes user-key friction entirely, but puts unbounded AI cost on a portfolio project and was already rejected in ADR-006. Voice is the one exception (bounded, owner-funded) because few users hold an ElevenLabs key.

---

## ADR-020: Pool Persistence via Draft → Published Lifecycle

**Status:** Accepted
**Date:** 2026-06-25

**Refines ADR-007** (Public AI-Generated Question Pool). ADR-007's policy — AI-generated questions live in a public pool with preserved attribution and community moderation — stands unchanged. This ADR records the *mechanism* by which a generated question reaches the pool, and the resulting change to the generation endpoint's behavior.

### Context
Iteration 3.4 returns generated drafts to the client **without** `CorrectOptionIndex` (hard rule #4 — the correct answer never leaves the server). For a pooled question to be playable later, the correct index must be persisted. Because the client never receives it, the client cannot send it back on a "save" action. This forces a decision about *when* and *how* a draft is persisted. Two shapes were considered:

1. **Generation = publish (one step).** Clicking Generate writes straight to the public pool. Simplest, faithful to ADR-007's "first user pays for everyone," but every generation — including low-quality output — immediately pollutes the shared pool, with no author curation gate.
2. **Draft → Published (two steps).** Generation persists drafts privately (owned by the author, server holds the correct index), and an explicit publish action promotes them to the public pool.

### Decision
Generation persists each draft as a `PooledQuestion` in **`Draft`** status, owned by the generating user, with attribution (user, provider, timestamp) and the server-side correct option. An explicit **publish** action transitions a question to **`Published`**, at which point it is visible in the public pool. The correct option is stored server-side and is **never** serialized to any client (hard rule #4 holds in both states).

This **changes the behavior of `POST /api/ai/questions`**: it now writes (persists drafts) where in 3.4 it was side-effect-free. The response contract to the client is unchanged (still no answer key); only the server-side persistence is new. Recording this here satisfies hard rule #5 (no silent contract change).

### Consequences
- A `Status` field (`Draft`/`Published`) lives on the aggregate from day one; it is also the natural attachment point for the deferred moderation/flagging states (ADR-007) without a later reshape.
- The author gets a curation gate: generated noise stays private until deliberately published, keeping the shared pool cleaner before automated/community moderation exists.
- Generation now has a write side effect and a persistence dependency; the handler gains a repository and `IUserContext`.
- Voting, flagging, and the moderation queue remain deferred (ADR-007, Phase 3/4). Playing pooled questions as quizzes is also deferred — 3.5 delivers persist + attribution + browse only.

### Alternatives Rejected
- **Generation = publish (one step)** — simplest and most faithful to ADR-007's cost framing, but removes the author curation gate and dumps every generation into the shared pool with no moderation yet in place. Rejected: a curation gate is cheap (one status field) and meaningfully protects pool quality in the interim.
- **Hold drafts in a cache / temp store keyed by a generation id** — keeps generation side-effect-free and defers the write to an explicit save, but adds an ephemeral-state mechanism (expiry, eviction) that the `Draft` status models more durably for free. Rejected as redundant complexity.

---

## ADR-021: Review Sessions Persist and Feed the Spaced-Repetition Queue

**Status:** Accepted
**Date:** 2026-06-30

**Supersedes the stateless-grading decision of iteration 2.6** (decision (a) in `.ai/sprints/sprint2/2.6-daily-review-ui.md`). That decision is reversed here; the rest of 2.6 stands.

### Context
Iteration 2.6 shipped the daily-review UI with **stateless** grading: `POST /api/review/grade` was a pure function of (answers, questions) and persisted nothing. Two problems surfaced in use:

1. **No trace.** A completed review left no record — no history, no way to know a review was done.
2. **No feedback loop.** The queue is computed (since 2.5) from quiz-attempt history: "latest answer per question, keep only those last answered wrong." Because a review wrote nothing, completing it did **not** change the queue — the same questions resurfaced every day until the user happened to answer them correctly in a *real* quiz. There was also no "done today" signal, so the banner kept nagging after a finished review.

The 2.6 decision deliberately avoided persistence to keep quiz History/Dashboard aggregates clean and to avoid modelling a review as a `QuizAttempt` (a review mixes questions from several quizzes and has no single `quizId`).

### Decision
Reviews are **persisted to their own aggregate**, `ReviewSession`, in a dedicated table — **never** as a `QuizAttempt`. `POST /api/review/grade` now **writes** a `ReviewSession` (with per-question items: `QuestionId`, `SelectedOptionId`, `AnsweredAt`) as a side effect before returning the same per-question results as before. The response contract is unchanged; only server-side persistence is new (recording this satisfies hard rule #5).

Review outcomes **feed the spaced-repetition queue** by widening the candidate source: the queue's candidate read unions answers from quiz attempts **and** review sessions, then runs the unchanged 2.5 `ReviewSelector`. A question answered correctly in a review becomes its latest answer and drops out of the next queue; answered wrong, it returns. Correctness is **derived** at read time from `Option.IsCorrect` (consistent with 2.5), not stored.

Keeping `ReviewSession` separate from `QuizAttempt` preserves the *intent* of the 2.6 decision — quiz History and Dashboard aggregates read only `QuizAttempts` and are unaffected — while restoring the persistence and feedback the feature needs.

### Consequences
- A new `ReviewSession` table and EF migration — the first schema change since the review feature began (2.5/2.6 added none).
- Review gains its own surface: a `GET /api/review/stats` read (sessions, questions reviewed, accuracy, current + best **review streak**) and a Dashboard stats tile. The review streak reuses a shared `StreakCalculator` extracted from the Dashboard quiz-streak logic (UTC days, one-day grace) so the two streaks stay in parity.
- "Done today" is derivable: a `ReviewSession` with `CompletedAt` on today's UTC date gates the daily banner.
- Grading is now a write; the frontend treats it as a mutation and invalidates the daily-queue and stats queries on success.
- Hard rule #4 is untouched: `GET /api/review/daily` still omits correctness; it is revealed only by `/grade` after submit.
- Milestones / achievement badges for review streaks are deferred to iteration 2.8.

### Alternatives Rejected
- **Stay stateless (the 2.6 status quo).** Simplest, but leaves the queue with no feedback loop and no done-today signal — the same questions resurface indefinitely. Rejected: it makes the spaced-repetition engine cosmetic.
- **Record review answers into the existing `Answer`/`QuizAttempt` history** so the selector picks them up for free. Achieves the feedback loop with no new table, but pollutes quiz History and Dashboard aggregates with non-quiz activity and forces a synthetic `quizId`. Rejected: violates the clean separation that motivated the 2.6 decision.
- **Rewrite `ReviewSelector` to take review outcomes as a second input.** More explicit, but duplicates the latest-answer-per-question logic and changes proven 2.5 code. Rejected in favour of unioning the candidate source, which reuses the selector unchanged.
