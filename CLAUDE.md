# CLAUDE.md

This file is the entry point for AI coding assistants (Claude Code, Cursor, Copilot Workspace) working on this repository. It captures the minimum context required to be useful here without re-reading every doc.

If you're an AI agent: read this file fully, then continue with `docs/ARCHITECTURE.md` and the relevant iteration file in `.ai/sprints/`. Do not skip those.

If you're a human: this file is also a useful single-page summary of how the project operates.

---

## Project context

TechQuiz is a self-knowledge testing platform for developers. Users take multiple-choice quizzes across technical categories (C#, ASP.NET, EF Core, SQL, etc.), track progress over time, and — eventually — generate questions using AI providers they bring keys for.

The project is built as a portfolio piece demonstrating:
- Clean Architecture in .NET 9 with TDD on Domain and Application layers
- React + TypeScript frontend with dual-theme (dark/light) design system
- Multi-user auth (Identity + JWT)
- AI integration via provider abstraction (Phase 3)
- Real CI/CD pipeline with a live deployment on Render + Neon

Current status: **all four phases are delivered and the app is live** at
[techquiz-web.onrender.com](https://techquiz-web.onrender.com). No planned iteration remains — further work
is off-plan (ad-hoc polish, content expansion, or a new initiative), so don't assume a "next iteration"
exists; ask the owner what to pick up.

The roadmap's four phases (see `docs/DECISION_LOG.md` ADR-013) — all `done`:
- Phase 0 — Foundation (setup, auth scaffolding, Docker, CI)
- Phase 1 — MVP (full quiz flow with hardcoded seed questions)
- Phase 2 — Dashboard with bento grid + spaced repetition
- Phase 3 — AI integration + code questions
- Phase 4 — Polish + deployment (incl. the "Momentum" redesign, gamification, a11y and performance passes)

---

## Tech stack

| Layer | Technology |
|---|---|
| Backend runtime | .NET 9 |
| API framework | ASP.NET Core 9 |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Mediator | MediatR |
| Validation | FluentValidation |
| Auth | ASP.NET Identity + JWT bearer + refresh tokens |
| Logging | Serilog (Console + Seq in dev, Console in the deployed environment) |
| Backend tests | xUnit + NSubstitute + Testcontainers (PostgreSQL) |
| Frontend runtime | Node.js 20 |
| Frontend tooling | Vite, pnpm 9 |
| Frontend framework | React 19 + TypeScript |
| Styling | Tailwind CSS with CSS variables for theming |
| Routing | React Router v6 |
| Data fetching | TanStack Query (React Query) |
| Forms | react-hook-form + zod |
| Charts | none — dashboard visuals are hand-rolled CSS/SVG bars (Recharts was dropped in 4.5) |
| Editor (Phase 3+) | Monaco Editor |
| Containerization | Docker + docker-compose |
| CI/CD | GitHub Actions + semantic-release |
| Host (live) | Render (Docker web services, `render.yaml`) + Neon managed PostgreSQL — see ADR-022 |

Exact versions: see `*.csproj` files and `web/package.json`. This table is a quick reference, not authoritative.

---

## Repository structure

```
TechQuiz/
├── CLAUDE.md                       ← this file
├── README.md                       ← portfolio-facing overview
├── TechQuiz.sln                    ← .NET solution
├── docker-compose.yml              ← local dev environment
├── commitlint.config.cjs           ← commit message validation
├── .releaserc.json                 ← semantic-release config
├── .editorconfig                   ← code style across editors
│
├── src/
│   ├── TechQuiz.Domain/            ← entities, value objects, rules (no framework deps)
│   ├── TechQuiz.Application/       ← use cases, MediatR handlers, validators, DTOs
│   ├── TechQuiz.Infrastructure/    ← EF Core, repositories, external integrations
│   └── TechQuiz.Api/               ← ASP.NET Core host, controllers, middleware
│
├── tests/
│   ├── TechQuiz.Domain.Tests/      ← pure unit tests (TDD-driven)
│   ├── TechQuiz.Application.Tests/ ← handler tests with mocked dependencies (TDD-driven)
│   └── TechQuiz.Infrastructure.Tests/ ← integration tests with Testcontainers
│
├── web/                            ← React frontend (Vite + TypeScript)
│   ├── src/
│   ├── package.json
│   └── ...
│
├── docs/                           ← portfolio-facing documentation
│   ├── ARCHITECTURE.md             ← system architecture + component patterns
│   ├── DECISION_LOG.md             ← ADRs (26 entries — read these to understand "why")
│   ├── DEPLOYMENT.md               ← Render + Neon runbook
│   ├── media/                      ← rendered walkthrough GIF used by the README
│   ├── CI_CD.md                    ← CI/CD pipeline description
│   ├── mockups/                    ← UI mockups as standalone .html files
│   └── postman/                    ← Postman collection for API testing
│
├── .ai/                            ← operational docs for AI assistants
│   ├── README.md                   ← what this folder is for
│   └── sprints/
│       ├── sprint0/                ← Phase 0 iteration files
│       ├── sprint1/                ← Phase 1 (MVP) iteration files — detailed
│       ├── sprint2/README.md       ← Phase 2 outline
│       ├── sprint3/README.md       ← Phase 3 outline
│       └── sprint4/README.md       ← Phase 4 outline
│
└── .github/
    ├── workflows/
    │   ├── ci.yml                  ← build + test + lint on PR
    │   ├── release.yml             ← semantic-release on merge to master
    │   └── api-smoke.yml           ← smoke-checks the deployed API
    │                                 (no deploy workflow: Render auto-builds on push to master)
    ├── BRANCH_PROTECTION.md        ← branch protection rules (manual GitHub setup)
    └── PULL_REQUEST_TEMPLATE.md
```

---

## Where to find decisions

Every non-trivial decision is documented. Before introducing a new pattern, library, or convention, check whether it's already decided:

- **`docs/DECISION_LOG.md`** — 26 Architecture Decision Records covering tech choices, scope strategy, UI/UX patterns, and CI/CD. This is the canonical source for "why is it like this?"
- **`docs/ARCHITECTURE.md`** — system structure, component patterns (code blocks, status pills, metric cards, etc.), and visual design system
- **`docs/CI_CD.md`** — pipeline behavior, deploy strategy, branch protection rationale
- **`.ai/sprints/sprintN/X.Y-*.md`** — each iteration's goal, definition of done, and ordered task list, plus `LOG.md` per sprint

If a question isn't answered in these files, ask the project owner before improvising.

---

## How we work

### Branch flow

- `master` is protected. All changes go through pull requests.
- Feature branches: `feat/short-description`, `fix/short-description`, `docs/short-description`
- Squash-merge to `master` (linear history enforced)
- After merge, the feature branch is auto-deleted by GitHub

### Commits and PR titles

Conventional Commits, enforced by commitlint locally (husky hook) and in CI:

```
feat: add keyboard shortcuts to quiz screen
fix(auth): refresh token rotation
docs: update architecture diagram
refactor(scoring): extract difficulty multiplier
test(domain): add edge cases for QuizAttempt
chore(deps): bump Microsoft.AspNetCore.Identity
```

Type list: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`. Scope is optional but encouraged. Header max 100 chars.

PR title must follow the same format — it becomes the squash commit message and feeds semantic-release.

### Iteration workflow

Every planned iteration is `done`, so there is no "current" one to pick up — confirm the goal with the
owner before starting. Work still ships the same way, and anything sizeable gets its own iteration file:

1. Open (or write) the iteration file in `.ai/sprints/sprintN/X.Y-*.md` and read its goal + DoD
2. Tasks are ordered. Work through them sequentially unless dependencies allow parallelism
3. After each meaningful chunk: commit with conventional commit message, push, open PR
4. PR triggers CI. When green, squash-merge
5. When all DoD checkboxes are met, update iteration status to `done` in the file's frontmatter
6. Record what happened in `.ai/sprints/sprintN/LOG.md` (Polish, newest entry first)

### Issue tracking

**Every commit lands with a corresponding GitHub issue. 1 issue ↔ 1 commit.**

Flow:
1. Before each commit, create the issue: `gh issue create --title "..." --label "type:..." --label "iteration:X.Y" --label "phase:N" --body "..."`. The body explains the *intent* of the change in a few lines.
2. Do the work, then commit with `Closes #N` in the commit message body (not the subject line):
   ```
   feat(infra): wire AppDbContext with EF mappings

   Closes #12
   ```
3. The PR body collects every `Closes #N` line so a single squash merge auto-closes the whole batch. Repeating the closures in the PR body is intentional — it's the most reliable trigger for GitHub's auto-close.

Labels (three orthogonal dimensions, applied as appropriate):
- `phase:N` — `phase:0` through `phase:4`. Always set.
- `iteration:X.Y` — `iteration:1.1` through `iteration:1.8` (more added per phase). Set when the issue belongs to a planned iteration; omitted for off-plan repo work (docs sync, tooling, ad-hoc chores).
- `type:feat | fix | docs | chore | test | refactor` — mirrors the commit type. Always set.

History note: issue tracking started at **iteration 1.3**. Earlier iterations (0.1, 1.1, 1.2) are traceable via PRs and iteration files only — no retroactive issues were created.

Off-plan work (e.g. docs syncs, dependency bumps, repo config) still gets an issue. The discipline rule is *"if you commit, you have an issue"* — exceptions are how slipshod tracking starts.

### Testing rules

- **Domain layer**: TDD strictly. Write failing test, make it pass, refactor. Aim for ≥90% coverage. No EF Core, no JSON, no HTTP — pure C#.
- **Application layer**: TDD with mocked repositories. NSubstitute for mocks. Test handler behavior, validation, and authorization checks.
- **Infrastructure layer**: integration tests against Testcontainers PostgreSQL. Verify EF Core mappings, repository round-trips, and migration application.
- **API layer**: minimal coverage. Controllers are thin wrappers around MediatR — meaningful behavior is tested at the Application layer. Smoke tests via `WebApplicationFactory<Program>` are enough.
- **Frontend**: no test mandate in MVP. Add component tests with Vitest + Testing Library in Phase 4 polish if time allows.

### Code style

- C#: file-scoped namespaces, primary constructors where they help, nullable reference types on. No StyleCop suppressions without an ADR.
- TypeScript: strict mode, no `any` (use `unknown` and narrow), prefer `type` over `interface` unless declaration merging is needed.
- Naming: PascalCase for C# types and methods, camelCase for TS identifiers, kebab-case for file names in web (`quiz-page.tsx`).
- Folder organization in web: feature folders (`features/quiz/`, `features/auth/`) over technical folders (`components/`, `hooks/`). Shared primitives live in `web/src/components/ui/` and `web/src/lib/`.

---

## Hard rules

These rules are non-negotiable. Violating any of them should fail review.

1. **Never commit secrets.** No API keys, JWT signing keys, DB passwords, or connection strings in source. Use `dotnet user-secrets` for local dev and GitHub Secrets for CI.
2. **Never bypass branch protection.** No force push to `master`, no direct commits to `master`, no merging without green CI.
3. **Never reference Infrastructure from Application or Domain.** Clean Architecture dependency rule (see ADR-001).
4. **Never expose `IsCorrect` on options through the API when serving an active quiz.** Doing so reveals answers to the client. The `QuestionDto` returned during a quiz must omit this field.
5. **Never silently change an ADR.** If a previously-recorded decision needs to change, append a new ADR explaining why and mark the old one as superseded. Editing in place loses history.
6. **Never add a library without justification.** Before introducing a new dependency, check whether existing stack solves the problem. If a new dep is needed, mention it in the PR description.
7. **Never skip the iteration file.** Before starting work, read the relevant iteration's goal and DoD. Don't work from memory or guesses about "what's next" — and since the roadmap is complete, confirm the goal with the owner rather than inventing the next iteration.

---

## Soft preferences

These are strong defaults — deviate only with explicit justification.

1. **Prefer prose docs over inline comments.** A method that needs comments to be understood probably needs a clearer name or smaller scope. Save explanatory text for `///` XML docs on public APIs.
2. **Prefer composition over inheritance.** Domain entities use value objects. Application handlers use injected services. Inheritance is rare and intentional.
3. **Prefer explicit return types in C#.** `var` is fine inside methods, but public APIs should declare types.
4. **Prefer named functions over arrow functions in TS** at the module level. Arrow functions inside hooks and callbacks are fine.
5. **Prefer fewer, larger components in React** until duplication forces extraction. Premature componentization fragments the codebase more than it helps.
6. **Match the mockups precisely** for UI work. The mockups in `docs/mockups/` aren't suggestions — they're decisions. Adapt for responsive breakpoints, but don't redesign without discussion.

---

## Quick task guide

### Adding a new feature

1. Agree the goal with the owner, then find or create the iteration file in `.ai/sprints/`
2. Read the iteration's goal and DoD
3. Branch: `feat/short-description`
4. Implement following the iteration's task list
5. Add tests (TDD for Domain/Application)
6. Update iteration file's DoD checkboxes as work completes
7. Open PR with Conventional Commits title

### Fixing a bug

1. Reproduce the bug — write a failing test first
2. Branch: `fix/short-description`
3. Make the test pass with minimal change
4. Run full test suite locally before pushing
5. PR title: `fix: short description of the bug`

### Modifying an ADR

Do not edit existing ADRs except to mark them superseded. To change a previous decision:
1. Add a new ADR at the end of `docs/DECISION_LOG.md` with the next number
2. In the new ADR, reference the superseded one and explain the change
3. In the old ADR, add a header line: `**Superseded by ADR-XYZ**`

### Adding a new dependency

1. Check whether the existing stack solves the problem
2. If new dep is required, prefer well-maintained, widely-used libraries
3. In the PR description, justify the choice — what problem does it solve, what alternatives were considered
4. If the dep introduces a significant pattern shift, write an ADR

### Working on UI

1. Open the corresponding mockup in `docs/mockups/` (e.g., `quiz-multiple-choice-dark.html`)
2. Open in browser, inspect colors and spacing via DevTools
3. Match exactly. Theme tokens are CSS variables — use the existing variables, don't hardcode
4. Verify in both dark and light themes before opening a PR

---

## Mockup reference

UI mockups exist as standalone HTML files in `docs/mockups/`. Each MVP screen has both dark and light variants. The Dashboard (Phase 2) additionally has an empty-state variant.

```
docs/mockups/
├── login-dual-theme.html           ← dark + light side by side in one file
├── categories-dark.html
├── categories-light.html
├── quiz-multiple-choice-dark.html
├── quiz-multiple-choice-light.html
├── quiz-code-output-dark.html      ← Phase 3 forward-look
├── quiz-code-output-light.html     ← Phase 3 forward-look
├── result-dark.html
├── result-light.html
├── dashboard-dark.html
├── dashboard-light.html
└── dashboard-empty-state.html
```

These are reference, not implementation. When building React components, match the visual output but use the project's design tokens (CSS variables from `web/src/styles/`) rather than hardcoded values from the mockup's inline styles.

---

## Project owner — work style notes

The project owner is a .NET developer working solo on this portfolio piece. The following notes help an AI assistant collaborate effectively:

- **Polish is the working language for live discussions**, but all repository artifacts (code, commits, docs, ADRs, iteration files) are in English. If the owner writes a message in Polish, respond in Polish but keep any artifacts produced in the conversation in English unless explicitly told otherwise.
- **TDD discipline matters.** When working on Domain or Application code, write the failing test before the implementation. Don't skip this step "for speed" — it's the discipline the iteration files codify.
- **The owner prefers recommendations over open questions.** When facing a non-trivial choice (library selection, design trade-off, refactor approach), recommend a concrete option with brief reasoning rather than presenting multiple options for the owner to weigh. The owner will push back if a recommendation doesn't fit.
- **Prefer reading existing files over asking.** Most context-establishing questions ("what tech stack?", "what's the auth flow?", "what are the categories?") are answered by `docs/ARCHITECTURE.md` and `docs/DECISION_LOG.md`. Read first, ask later — and only about genuinely undetermined items.
- **Owner is learning React in parallel with .NET work.** When working on `web/`, slightly more guidance and explanation is welcome — explain why a hook is structured a certain way, why a pattern (compound components, render props, etc.) is being used. Backend code can assume strong .NET familiarity.
- **Pragmatism over purism.** The owner appreciates principled architecture (Clean Architecture, TDD on domain) but is wary of over-engineering. If a pattern feels like ceremony without benefit, push back rather than implement it dutifully.

---

## Known gotchas

Things that have bitten before or are easy to get wrong:

- **EF Core migrations and `IDesignTimeDbContextFactory`** — the Infrastructure project needs a design-time factory so `dotnet ef` commands work without booting the full API. Without it, migrations fail with cryptic errors. See `Infrastructure/Persistence/DesignTimeDbContextFactory.cs` (will exist after iteration 1.3).
- **PostgreSQL casing** — entity names are PascalCase in code but snake_case in DB. The DbContext applies a global naming convention. Don't manually quote table names.
- **JWT in localStorage is a vulnerability.** This project uses memory-only JWT + HttpOnly refresh cookie. Don't "simplify" by moving JWT to localStorage.
- **Code blocks stay dark in light mode.** This is intentional (see ADR-016). Syntax highlighting reads better on dark, and dev tools train the eye to expect dark code regions. Don't "fix" this.
- **Quiz screen hides the topbar.** The `<AppShell>` component is route-aware and renders only the `<Outlet />` on `/quiz/:id` routes. Don't add the shell to the quiz route in routing config.
- **The seeder is idempotent per resource, with one deliberate exception.** Tracks, categories, questions and the demo user each have their own existence check, so re-running is safe. The demo account's *quiz history* is the exception: it is wiped and regenerated on every boot with dates relative to now, so the live demo never shows a stale dashboard. Only the demo user is touched. To reset content locally, `docker compose down -v`.
- **Public registration is closed** behind `Auth:RegistrationEnabled` (`false` in base appsettings so the live API returns 403; `true` in `appsettings.Development.json` so local work and integration tests still register). Reversible via the flag once a privacy policy exists.
- **Render free instances sleep after ~15 minutes.** The first request after idle can take 30–50 seconds. This is acceptable for portfolio demos; the README says so up front, so reviewers aren't confused.

---

## Communication

Working with the project owner: respond in **Polish** if the owner writes in Polish, **English** if the owner writes in English. Either language is fine for the conversation, but all committed artifacts (code, commits, docs, iteration updates) stay in English regardless.

When uncertain about scope, intent, or architectural direction: ask before implementing. Wasted code is worse than a clarifying question.
