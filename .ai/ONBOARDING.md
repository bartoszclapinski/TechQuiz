# ONBOARDING — TechQuiz

> Last updated: 2026-05-05
> Purpose: Full project context for AI assistants and new contributors.

---

## Project Overview

**TechQuiz** is a personal knowledge testing platform for software engineering topics — inspired by Pluralsight Skill IQ. It uses AI to generate questions from study materials, evaluates code answers, and tracks progress over time with charts and spaced repetition.

### Two purposes (in priority order):

1. **Personal study tool** — used daily to prepare for technical interviews, especially the EPAM .NET interview in June 2026
2. **Portfolio project** — demonstrates Clean Architecture, AI integration, full-stack skills, testing discipline

### Why this matters for the design:

The project should be **shippable fast** (MVP in 2 weeks for daily use during the EPAM sprint), then **polished gradually** for portfolio quality. We optimize for "useful tool first, beautiful code second" — but never sloppy code.

---

## Author

**Bartosz Cłapiński** — Computer Science engineer (PJATK 2024), .NET developer with 10 years of management background, transitioning to software engineering full-time.

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
- **Entity Framework Core 9** + **PostgreSQL**
- **MediatR** — CQRS handlers
- **FluentValidation** — input validation
- **Serilog** — structured logging
- **xUnit** + **Moq** — testing

### Frontend
- **React 18** + **TypeScript**
- **Vite** — build tool
- **TanStack Query** — server state
- **Recharts** — progress charts
- **Monaco Editor** — code editor in browser (for code questions)
- **Tailwind CSS** — styling

### AI Integration
- **OpenAI API** — question generation, code answer evaluation

### DevOps
- **Docker** + **docker-compose**
- **GitHub Actions** — CI (build, test, lint)
- **PostgreSQL** in container

### Why this stack:
- C# / ASP.NET Core: author's primary language, target tech for job hunt
- React / TypeScript: most demanded frontend combo in .NET job offers (fills a gap in author's portfolio — already has Blazor and Angular projects)
- PostgreSQL: free, robust, Docker-friendly
- Clean Architecture: demonstrates architectural maturity to recruiters
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
│  TechQuiz.API (ASP.NET Core)            │  Presentation (API)
│  • Controllers, Filters, Middleware     │
└─────────────────────────────────────────┘
                  │
┌─────────────────────────────────────────┐
│  TechQuiz.Application                   │  Use cases
│  • CQRS Commands/Queries (MediatR)      │
│  • Validators (FluentValidation)        │
│  • DTOs, mappers                        │
└─────────────────────────────────────────┘
                  │
┌─────────────────────────────────────────┐
│  TechQuiz.Domain                        │  Business rules
│  • Entities, Value Objects, Enums       │
│  • Domain interfaces (IRepository, ...) │
│  • Domain events                        │
└─────────────────────────────────────────┘
                  ▲
                  │ implements
┌─────────────────────────────────────────┐
│  TechQuiz.Infrastructure                │  External concerns
│  • EF Core DbContext, migrations        │
│  • Repository implementations           │
│  • OpenAI client                        │
│  • External services                    │
└─────────────────────────────────────────┘
```

### Dependency rule:
Dependencies point **inward**. Domain depends on nothing. Application depends only on Domain. Infrastructure and API depend on Application + Domain.

### Folder structure:
```
src/
├── TechQuiz.Api/              # ASP.NET Core Web API
├── TechQuiz.Application/      # Business logic, CQRS, MediatR
├── TechQuiz.Domain/           # Entities, VOs, interfaces
└── TechQuiz.Infrastructure/   # EF Core, AI providers, persistence

tests/
├── TechQuiz.Domain.Tests/         # Pure unit tests (TDD-driven)
├── TechQuiz.Application.Tests/    # Handler tests with mocked deps (NSubstitute)
└── TechQuiz.Infrastructure.Tests/ # Integration tests (Testcontainers)

web/                                # React + TypeScript SPA (Vite, pnpm) — separate from .NET solution
```

---

## Roadmap (4 phases)

### Phase 1 — MVP (~3 building sessions, weeks 1-2 of EPAM sprint)
**Goal:** Working quiz with multiple choice questions and score.

- Solution structure, Clean Architecture skeleton
- Domain: Quiz, Question, Answer, Category entities
- Application: Quiz CRUD, scoring logic
- API: GET categories, GET quiz by category, POST submit answers
- Infrastructure: PostgreSQL via Docker, EF Core migrations, seed data
- Frontend: category selection → quiz screen → result screen
- Markdown parser: imports questions from EPAM course quiz files

**Deliverable:** Author can run the app, take a quiz, see score.

### Phase 2 — History & Charts (weeks 3-4)
**Goal:** Track progress over time, visualize, daily review.

- Domain: QuizAttempt, UserProgress entities
- API: history, stats per category
- Frontend: Dashboard with line chart (scores over time) + radar chart (skill map)
- Spaced repetition algorithm (SM-2 simplified) — wrong-answered questions return more often
- "Daily Review" mode

**Deliverable:** Charts work, author sees progress trends, gets reminded of weak areas.

### Phase 3 — Code Questions + AI (weeks 5-7)
**Goal:** Code questions with Monaco Editor + AI generates and evaluates.

- Domain: CodeQuestion entity (snippet, expected output, explanation)
- Frontend: Monaco Editor integration with C# syntax highlighting
- Question types: "What does this code return?" / "Find the bug" / "Complete the code"
- Infrastructure: OpenAI service
- AI Generator: produces questions on demand based on topic + difficulty
- AI Evaluator: assesses code answers (not just exact match) with feedback

**Deliverable:** Author can solve code questions and get AI feedback.

### Phase 4 — Polish & Gamification (weeks 8-10)
**Goal:** Production-quality app for portfolio.

- Gamification: XP, levels, badges, streaks
- Difficulty adaptation
- Timed mode (interview simulation)
- PDF export of "skill report"
- Tests: 70%+ coverage on Application layer
- Docker compose: full stack containerized
- GitHub Actions: CI pipeline (build + test + lint)
- README with screenshots, architecture diagram

**Deliverable:** Public, polished repo ready to show recruiters.

---

## Current Status

**As of 2026-05-05:** Pre-implementation. README and `.gitignore` in place. First building session: Wednesday 2026-05-06.

Check the latest commits and `git log` for current state.

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
- **Naming pattern:** `Method_Scenario_ExpectedBehavior`
  - `Add_TwoPositiveNumbers_ReturnsSum`
  - `Withdraw_AmountExceedsBalance_ThrowsException`
- **AAA pattern** with comments for clarity in early tests, can be omitted once obvious
- **One Act per test**
- **No infrastructure** in unit tests (no DB, no network) — use mocks/fakes
- **Integration tests** use TestContainers (real PostgreSQL in Docker)

### Commits
- **Format:** `<type>: <description>` (Conventional Commits style)
- **Types:** `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`
- **Examples:**
  - `feat: add quiz scoring logic`
  - `test: cover edge cases in IsPrime`
  - `refactor: extract question repository`
  - `docs: update onboarding with phase 2 details`
- **Why "what" matters less than "why":** if the change rationale isn't obvious from the diff, explain in the commit body.

### Branches
- **`master`** — always green (CI passes, app runs), protected, never pushed to directly
- **`feature/short-description`** — new features (squash-merged)
- **`fix/short-description`** — bug fixes
- **Discipline:** never commit directly to `master`, not even trivial changes. Every change goes through a feature branch + PR + squash merge. One iteration = one PR.

### Pull Requests
- Even solo: open PRs for non-trivial changes. Forces self-review.
- PR description: summary + test plan
- Squash on merge

---

## Local Setup

### Prerequisites
- .NET 9 SDK
- Node.js 20+ (for `TechQuiz.Web`)
- Docker Desktop (for PostgreSQL container)
- Git

### Run locally
```powershell
# Clone (when published)
git clone https://github.com/bartoszclapinski/TechQuiz.git
cd TechQuiz

# Start PostgreSQL
docker compose up -d postgres

# Apply migrations
dotnet ef database update --project src/TechQuiz.Infrastructure --startup-project src/TechQuiz.API

# Run API
dotnet run --project src/TechQuiz.API

# Run Web (in separate terminal)
cd src/TechQuiz.Web
npm install
npm run dev
```

### Environment variables
- See `appsettings.Development.json.example` (gitignored real version)
- `OPENAI_API_KEY` — required for Phase 3+
- `ConnectionStrings__Postgres` — overrides default Postgres connection

---

## How AI Should Help

### When asked to add a feature:
1. Look at the active phase in this file — does the feature fit current scope?
2. If yes: scope minimally, prefer extending existing patterns to creating new ones
3. If it's beyond scope: ask whether to defer or expand the phase

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

## External Context

- This project was conceived as a study tool for the EPAM .NET interview (June 2026)
- The author maintains separate private notes/planning documents (not in this repo)
- The course materials feeding into the question bank are from EPAM Fundamentals .NET CEE & TR #2

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
