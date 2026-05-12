# QUICK START — Paste into a new chat

> Short context for new AI chat sessions. For full details: `.ai/ONBOARDING.md`.

---

## Project: TechQuiz (30-second context)

Personal knowledge testing platform — like Pluralsight Skill IQ. Used as:
1. **Daily study tool** for technical interview prep (.NET, EPAM)
2. **Portfolio piece** — Clean Architecture + AI integration

**Stack:** ASP.NET Core 9 + EF Core + PostgreSQL + React/TS + OpenAI + Docker.

**Architecture:** Clean Architecture — Domain ← Application ← (API + Infrastructure) — Presentation: React SPA.

```
src/
├── TechQuiz.API/              # Web API
├── TechQuiz.Application/      # CQRS, MediatR, validators
├── TechQuiz.Domain/           # Entities, VOs, interfaces
├── TechQuiz.Infrastructure/   # EF Core, OpenAI
└── TechQuiz.Web/              # React + TypeScript
tests/
├── TechQuiz.UnitTests/
└── TechQuiz.IntegrationTests/
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

## Roadmap (where we are)

| Phase | Goal |
|-------|------|
| 1 — MVP | Multiple choice quiz + score (weeks 1-2) |
| 2 — History | Dashboard, charts, spaced repetition (weeks 3-4) |
| 3 — Code + AI | Monaco editor, AI generates/evaluates (weeks 5-7) |
| 4 — Polish | Gamification, tests, Docker, CI (weeks 8-10) |

**Check `git log` for actual current state.**

---

## Conventions (cheat sheet)

- **Tests:** `Method_Scenario_ExpectedBehavior`, AAA, no infrastructure in unit tests
- **Naming:** PascalCase classes, `_camelCase` private fields, `IPrefix` interfaces
- **Async:** every IO is async, never `.Result`
- **Commits:** Conventional Commits (`feat:`, `fix:`, `test:`, `refactor:`, `docs:`)
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
