# QUICK START — Paste into a new chat

> Short context for new AI chat sessions. For full details: `.ai/ONBOARDING.md`.

---

## Project: TechQuiz (30-second context)

Self-knowledge testing platform for developers — multiple-choice quizzes and code challenges across technical categories, with progress tracking and spaced repetition. Multi-user (Identity + JWT); public registration is currently closed and the live demo uses a seeded demo account. Built as a **portfolio piece** — Clean Architecture, TDD, AI integration, real CI/CD.

**Stack:** .NET 9 / ASP.NET Core 9 + EF Core 9 + PostgreSQL 16 + React 19/TypeScript (Vite) + Docker. AI: provider abstraction (`IAiProvider`, bring-your-own key, ADR-006/ADR-019). Anthropic (Claude) is live; OpenAI is implemented (5.1) but stays "soon" in Settings until a funded-key run; Gemini and OpenRouter have no client yet. Hosted on Render + Neon (ADR-022).

**Architecture:** Clean Architecture — Domain ← Application ← (API + Infrastructure) — Presentation: React SPA.

```
src/
├── TechQuiz.Api/              # Web API
├── TechQuiz.Application/      # CQRS (MediatR), validators, ports in Abstractions/
├── TechQuiz.Domain/           # Entities, value objects, domain exceptions (no framework deps)
└── TechQuiz.Infrastructure/   # EF Core + seeder, Identity/JWT, AI providers + key store, Judge0 code execution
tests/
├── TechQuiz.Domain.Tests/
├── TechQuiz.Application.Tests/
├── TechQuiz.Api.Tests/        # WebApplicationFactory endpoint smoke tests
└── TechQuiz.Infrastructure.Tests/
web/                           # React + TypeScript SPA (Vite, separate from .NET solution)
```

---

## Author / Communication

**Bartosz Cłapiński** — github.com/bartoszclapinski

- **Chat:** Polish
- **Code/docs:** English
- **Style:** concrete, minimal emoji, no fluff
- **Pace:** small steps, verify each
- **Avoid:** speculative abstractions, bloat, comments explaining what code already shows

---

## Roadmap (status)

Phases 0–4 are **done** and the app is **live** at https://techquiz-web.onrender.com (Render + Neon; free tier sleeps, so the first request can take 30–50 s).

| Phase | Delivered |
|-------|-----------|
| 0 — Foundation | Solution skeleton, auth scaffolding, Docker, CI |
| 1 — MVP | Multiple-choice quiz flow + score, seeded questions |
| 2 — History | Dashboard (bento grid), history, spaced repetition, achievements |
| 3 — Code + AI | AI provider seam + BYO key (Anthropic live), AI question generation, public pool, Monaco code challenges + AI feedback |
| 4 — Polish | Mobile, gamification, a11y, performance, production deploy, taxonomy, "Momentum" redesign |
| 5 — Post-launch | *In progress* (ADR-027): 5.1 OpenAI, 5.2 Gemini, 5.3 OpenRouter providers; 5.4 auth hardening (#66, #67, #68, #99) |

Current work lives in `.ai/sprints/sprint5/`; confirm anything outside it with the owner before starting.

---

## Conventions (cheat sheet)

- **Tests:** unit tests `Method_Scenario_ExpectedBehavior` (AAA, NSubstitute mocks, no infrastructure); integration/API tests may use sentence-style names (`Generate_requires_authentication`); Testcontainers PostgreSQL for Infrastructure
- **Naming:** PascalCase types/methods, `_camelCase` private fields, `IPrefix` interfaces; prefer primary constructors (see `.editorconfig`)
- **Async:** every IO is async, never `.Result`
- **Commits:** Conventional Commits, header ≤ 100 chars, enforced by commitlint — types `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`; 1 issue ↔ 1 commit (`Closes #N` in the body)
- **Branches:** `feat/`, `fix/`, `docs/` … off `master`; PR + squash-merge, never push to `master`
- **Files:** one class per file, file-scoped namespaces, nullable enabled

---

## How to help

**Do:**
- Match existing file style
- Verify build + tests pass before saying "done"
- Ask before destructive operations
- Explain *why* (in commit/comment) when non-obvious

**Don't:**
- Add features beyond ask
- Write comments that just describe what code does
- Introduce abstractions for hypothetical futures
- Suggest renames without reason

---

*For more: `.ai/ONBOARDING.md`*
