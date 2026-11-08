# ONBOARDING — TechQuiz

> Last updated: 2026-11-08
> Purpose: Full project context for AI assistants and new contributors.

---

## Project Overview

**TechQuiz** is a self-knowledge testing platform for developers. Users take multiple-choice quizzes and code challenges across technical categories (C#, ASP.NET, EF Core, SQL, etc.). It generates questions with bring-your-own-key AI providers from a topic and difficulty, grades code challenges by sandboxed execution (Judge0) with optional AI feedback, and tracks progress over time with a dashboard and spaced repetition. It is multi-user (Identity + JWT); public registration is currently closed and the live demo uses a seeded demo account.

The project is built as a **portfolio piece** demonstrating Clean Architecture, TDD on Domain and Application, AI integration behind a provider abstraction, full-stack skills, and a real CI/CD pipeline with a live deployment. It was shipped in small iterations — a working tool first, polished gradually — but never with sloppy code.

---

## Author

**Bartosz Cłapiński** — .NET developer.

- GitHub: [github.com/bartoszclapinski](https://github.com/bartoszclapinski)
- LinkedIn: [linkedin.com/in/bartosz-clapinski](https://linkedin.com/in/bartosz-clapinski)

### Communication preferences (for AI):
- **Chat language:** Polish
- **Code/docs language:** English
- **Style:** Concrete, minimal emoji, professional
- **Approach:** Educational — explain trade-offs and teach patterns, don't just write code
- **Pace:** Small steps, verify each before moving on
- **Pet peeve:** Don't add features beyond what's requested. Don't speculate about future needs. Three similar lines beat a premature abstraction.

---

## Tech Stack

### Backend
- **ASP.NET Core 9** — Web API
- **Entity Framework Core 9** + **PostgreSQL 16**
- **MediatR** — CQRS handlers
- **FluentValidation** — input validation
- **Serilog** — structured logging
- **ASP.NET Identity** + **JWT bearer** + HttpOnly refresh-token cookie — auth
- **xUnit** + **NSubstitute** + **FluentAssertions** (pinned to 7.x) + **Testcontainers** (PostgreSQL) — testing

### Frontend
- **React 19** + **TypeScript**
- **Vite 8** — build tool
- **Tailwind CSS 3** — utility-first styling
- **TanStack Query** — server state
- **React Router v6** + **react-hook-form** + **zod** — routing and forms
- **Charts** — none; dashboard visuals are hand-rolled CSS/SVG (Recharts dropped in 4.5)
- **Monaco Editor** — code editor in browser (for code challenges)

### AI Integration
- **Bring-your-own-key AI providers** (ADR-006, ADR-019) — provider abstraction (`IAiProvider`) over Anthropic (the only implemented client); OpenAI, Gemini and OpenRouter exist in `AiProviderKind` but have no client yet (shown as "soon" in Settings); used for question generation and qualitative code feedback
- **Judge0** (self-hosted, `codeexec` compose profile) — sandboxed execution that grades code challenges (ADR-018)

### DevOps
- **Docker** + **docker-compose** — local stack (API, web, PostgreSQL, Seq)
- **GitHub Actions** — CI (build, test, lint)
- **Render** (Docker web services, `render.yaml`) + **Neon** managed PostgreSQL — live host (ADR-022, `docs/DEPLOYMENT.md`)
- **semantic-release** (`release.yml`) and **api-smoke.yml** (deployed-API smoke check)

### Why this stack:
- C# / ASP.NET Core: the author's primary language
- React / TypeScript: the most common frontend pairing for .NET backends
- PostgreSQL: free, robust, Docker-friendly
- Clean Architecture: keeps Domain and Application testable without infrastructure
- xUnit: most modern .NET testing framework

---

## Architecture

Clean Architecture with 4 layers + presentation:

```
┌─────────────────────────────────────────┐
│  web/ (React + TS, Vite)                │  Presentation (SPA)
└─────────────────────────────────────────┘
                  │ HTTP (REST)
┌─────────────────────────────────────────┐
│  TechQuiz.Api (ASP.NET Core)            │  Presentation (API)
│  • Controllers, exception handler       │
└─────────────────────────────────────────┘
                  │
┌─────────────────────────────────────────┐
│  TechQuiz.Application                   │  Use cases
│  • CQRS Commands/Queries (MediatR)      │
│  • Validators (FluentValidation)        │
│  • DTOs, ports in Abstractions/         │
│    (repositories, IAiProvider, ...)     │
└─────────────────────────────────────────┘
                  │
┌─────────────────────────────────────────┐
│  TechQuiz.Domain                        │  Business rules
│  • Entities, Value Objects, Enums       │
│  • Domain rules + domain exceptions     │
│  • No framework dependencies            │
└─────────────────────────────────────────┘
                  ▲
                  │ implements Application ports
┌─────────────────────────────────────────┐
│  TechQuiz.Infrastructure                │  External concerns
│  • EF Core DbContext, migrations        │
│  • Repository implementations           │
│  • Identity + JWT, data seeder          │
│  • AI providers (Anthropic), Judge0     │
└─────────────────────────────────────────┘
```

### Dependency rule:
Dependencies point **inward**. Domain depends on nothing. Application depends only on Domain. Infrastructure and API depend on Application + Domain.

### Folder structure:
```
src/
├── TechQuiz.Api/              # ASP.NET Core Web API
├── TechQuiz.Application/      # Use cases, CQRS (MediatR), validators, ports in Abstractions/
├── TechQuiz.Domain/           # Entities, VOs, domain rules
└── TechQuiz.Infrastructure/   # EF Core + seeder, Identity/JWT, AI providers + key store, Judge0 client

tests/
├── TechQuiz.Domain.Tests/         # Pure unit tests (TDD-driven)
├── TechQuiz.Application.Tests/    # Handler tests with mocked deps (NSubstitute)
├── TechQuiz.Api.Tests/            # API smoke tests (WebApplicationFactory)
└── TechQuiz.Infrastructure.Tests/ # Integration tests (Testcontainers)

web/                                # React + TypeScript SPA (Vite, pnpm) — separate from .NET solution
```

---

## Roadmap

The roadmap is defined in `docs/DECISION_LOG.md` ADR-013: Phase 0 Foundation → Phase 1 MVP → Phase 2 Dashboard + spaced repetition → Phase 3 AI + code questions → Phase 4 Polish + deployment. All phases are `done`; per-iteration detail lives in `.ai/sprints/sprintN/` (iteration files + `LOG.md`). The original pre-implementation plan (Markdown import, timed mode, PDF export, difficulty adaptation) was not carried forward.

---

## Current Status

All four roadmap phases are delivered and the app is live at https://techquiz-web.onrender.com (Render + Neon, ADR-022). Render free instances sleep after ~15 minutes, so the first request after idle can take 30–50 seconds.

No planned iteration remains; further work is off-plan, so confirm the goal with the owner first. Known open follow-ups (auth hardening, deferred since Phase 1): #66, #67, #68, #99 — see `gh issue list --state open`. See `CLAUDE.md` and `.ai/sprints/sprint4/LOG.md` for the latest state.

---

## Conventions

### Code style
- **Nullable reference types:** enabled project-wide
- **File-scoped namespaces:** `namespace TechQuiz.Domain;`
- **Primary constructors** for services where appropriate
- **Records** for DTOs and immutable value objects
- **Async-all-the-way** — every IO method is async, no `.Result` or `.Wait()`
- **One class per file**, file name matches type name

### Naming
- **Classes:** `PascalCase` (`QuizAttempt`)
- **Interfaces:** `IPascalCase` (`IQuizRepository`)
- **Methods/Properties:** `PascalCase` (`GetByIdAsync`)
- **Local variables / parameters:** `camelCase`
- **Private fields:** `_camelCase` (with underscore)
- **Constants:** `PascalCase` (not SCREAMING_SNAKE)

### Tests
- **Naming pattern:** `Method_Scenario_ExpectedBehavior` for unit tests
  - `Add_TwoPositiveNumbers_ReturnsSum`
  - `Withdraw_AmountExceedsBalance_ThrowsException`
- Integration/API tests may use sentence-style names (`Generate_requires_authentication`)
- **AAA pattern** with comments for clarity in early tests, can be omitted once obvious
- **One Act per test**
- **No infrastructure** in unit tests (no DB, no network) — use mocks/fakes
- **Integration tests** use TestContainers (real PostgreSQL in Docker)

### Commits
- **Format:** `<type>(<optional scope>): <description>`, header ≤ 100 chars, enforced by commitlint (husky + CI)
- **Types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`
- **Examples:**
  - `feat: add quiz scoring logic`
  - `test: cover edge cases in IsPrime`
  - `refactor: extract question repository`
  - `docs: update onboarding with phase 2 details`
- **Why "what" matters less than "why":** if the change rationale isn't obvious from the diff, explain in the commit body.

### Branches
- **`master`** — always green (CI passes, app runs), protected, never pushed to directly
- **`feat/short-description`** — new features (squash-merged)
- **`fix/short-description`** — bug fixes
- Other prefixes mirror the commit type: `docs/`, `chore/`, `ci/`, …
- **Discipline:** never commit directly to `master`, not even trivial changes. Every change goes through a feature branch + PR + squash merge. One iteration = one PR.

### Pull Requests
- Even solo: open PRs for non-trivial changes. Forces self-review.
- PR description: summary + test plan + every `Closes #N` line for the issues the PR resolves
- Squash on merge

### Issue tracking (from iteration 1.3 onwards)
- **1 issue ↔ 1 commit.** Create the issue with `gh issue create` before each commit, then put `Closes #N` in the commit message body.
- Labels: `phase:N` + `iteration:X.Y` (optional for off-plan work) + `type:feat|fix|docs|chore|test|refactor`.
- Off-plan work (docs syncs, tooling) still gets an issue — *if you commit, you have an issue*.
- Full rules: see `CLAUDE.md` → *How we work* → *Issue tracking*.

---

## Local Setup

### Prerequisites
- Docker Desktop (with WSL2 backend on Windows)
- Git
- For local development outside Docker: .NET 9 SDK + Node.js 20+ + pnpm 9

### Run locally
```powershell
# Clone
git clone https://github.com/bartoszclapinski/TechQuiz.git
cd TechQuiz

# Full stack (API + web + Postgres + Seq)
docker compose up -d
# /health → http://localhost:8085/health
# Web    → http://localhost:5173
# Seq UI → http://localhost:8081

# With the Judge0 code runner (needed for code challenges):
docker compose --profile codeexec up -d

# Or run the API on the host with Postgres + Seq in containers:
dotnet user-secrets set "Jwt:SigningKey" "<base64-256-bit>" --project src/TechQuiz.Api
# compose publishes Postgres on host port 5433, while appsettings.json defaults to 5432:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=techquiz;Username=techquiz;Password=techquiz_dev" --project src/TechQuiz.Api
docker compose up -d postgres seq
dotnet run --project src/TechQuiz.Api

# Web in dev mode (HMR):
cd web
pnpm install
pnpm dev
```

> In Development (and Staging) the API applies pending EF Core migrations and runs the idempotent seeder on startup, so no manual `dotnet ef database update` step is needed. To reset local data, `docker compose down -v`.

### Environment variables
- `src/TechQuiz.Api/appsettings.json` / `appsettings.Development.json` (both committed, no secrets); for a host run the JWT signing key comes from `dotnet user-secrets` (`Jwt:SigningKey`), while the compose stack sets a dev-only key itself
- `ConnectionStrings__DefaultConnection` — overrides the default Postgres connection (`appsettings.json`)
- AI keys are per-user (bring-your-own-key, ADR-006): entered in the app's Settings page and stored encrypted — no server-side AI key env var is needed

---

## How AI Should Help

### When asked to add a feature:
1. All roadmap phases are done — confirm the goal with the owner, then find or create the iteration file in `.ai/sprints/` (see `CLAUDE.md` → *Iteration workflow*).
2. Scope minimally: prefer extending existing patterns to creating new ones
3. If it's beyond the agreed scope: ask whether to defer or expand it

### Code review philosophy:
- Prefer **deletion over addition** — fewer lines = less to maintain
- Question every abstraction — does it pay for its weight in 3+ uses?
- Don't add error handling for impossible cases (trust internal contracts)
- Validate at boundaries (user input, external APIs), trust within

### Don't:
- Don't write extensive comments explaining what code does (the code shows that)
- Don't add unused fields, parameters, or imports "just in case"
- Don't introduce backwards-compatibility shims when the change is internal
- Don't write multi-paragraph docstrings; one short line max
- Don't suggest renaming things unless there's a real reason
- Don't add caching/optimization without measurement showing it's needed

### Do:
- Do explain *why* a non-obvious decision is made — in commit message or short comment
- Do ask before destructive operations (rm, force push, schema changes)
- Do verify build + tests pass before reporting "done"
- Do match the existing style of the file you're editing
- Do flag if you spot something off-topic (security issue, dead code, broken test) — but ask before fixing

---

## Glossary

| Term | Meaning |
|------|---------|
| **SUT** | System Under Test (the class being tested) |
| **Test double** | Generic term for stub/mock/fake/dummy |
| **Spaced repetition** | Algorithm that schedules reviews at increasing intervals (longer for well-known items) |
| **CQRS** | Command Query Responsibility Segregation — separate read and write models |
| **MediatR** | Library for in-process messaging (request → handler) |
| **Clean Architecture** | Architectural pattern by Robert C. Martin (Uncle Bob); dependency rule points inward |

---

*Welcome aboard. The code should always be more concrete than this document.*
