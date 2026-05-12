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

**Status:** Accepted
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
- Squash merges produce clean commits on `main`
- Enables future automation (changelog generation, semantic versioning) if needed
- Adds a small setup step in Phase 0 (npm init, commitlint install, husky setup)
- Demonstrates production-grade discipline to recruiters

---

## ADR-010: GitHub Flow with Squash Merge

**Status:** Accepted
**Date:** 2026-05-11

### Context
Branching strategies range from trunk-based (commit directly to `main`) to Git Flow (multiple long-lived branches). For a solo project, the question is whether the overhead of a feature-branch workflow is worth the discipline and portfolio signal.

### Decision
The project uses **GitHub Flow**: feature branches, pull requests with self-review, and squash merge to `main`. Branch protection rules on `main` enforce the workflow.

Branch naming follows the same vocabulary as commit types: `feature/short-description`, `fix/short-description`, `refactor/short-description`, etc.

### Consequences
- Every feature has a self-review checkpoint before merging
- `main` history stays clean with one commit per feature
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

**Branch protection.** `main` is protected. All changes go through pull requests. CI must pass before merge. Review approvals are not required (solo project). Squash-merge only, linear history enforced. Force pushes and deletions blocked.

**Required CI checks.** Three jobs run on every PR: backend (`dotnet build` + `dotnet test` against ephemeral PostgreSQL service), frontend (`pnpm lint` + `pnpm build`), and commitlint (PR title + every commit in PR validated against Conventional Commits).

**Semantic-release.** Automated versioning and CHANGELOG generation enabled from the start, not deferred. Triggered on every push to `main`. Commit types determine version bump (`feat:` → minor, `fix:` → patch, `BREAKING CHANGE` → major). Releases create Git tags and GitHub Releases automatically.

**Staging deployment.** Azure App Service (Linux, .NET 9 runtime) chosen as deploy target instead of Fly.io, Railway, or Render. The rationale is portfolio fit: this project targets .NET developer roles, and Azure familiarity is a directly relevant signal for those positions. Staging deploys are scheduled as iteration 1.8, after Phase 1 (MVP) feature work completes.

**Production deployment.** Deferred to Phase 4. The same Azure pipeline pattern will be reused; a separate `production` environment in GitHub will require manual approval and pin to git tags.

**Secret management.** GitHub Secrets at the repository level for shared values (Azure credentials). GitHub Environments (`staging`, eventually `production`) for environment-specific overrides. Local development uses `dotnet user-secrets` — no secrets committed.

### Consequences
- Every commit on `main` represents a passing build — `main` is always deployable
- Squash-merge keeps history readable: one commit per feature on `main`, full granularity preserved in PR
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
