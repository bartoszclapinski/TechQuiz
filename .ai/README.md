# `.ai/` — Operational documents for AI assistants

This folder contains working documents intended for AI coding assistants (Claude Code, Copilot, Cursor) and for the project owner. Unlike `docs/`, which is portfolio-facing and describes the project at a high level, `.ai/` is **operational** — it tells whoever opens this repository exactly what to do next.

## Why is this folder public?

This project is built in active collaboration with AI assistants. The `.ai/` folder exists as a deliberate, visible part of the workflow rather than something hidden. It demonstrates how the codebase is planned and how implementation work is structured.

If you're an AI assistant reading this: start with `CLAUDE.md` at the repository root, then come back here.

## Structure

```
.ai/
├── README.md               ← you are here
└── sprints/
    ├── sprint0/            ← Phase 0: Foundation
    │   └── 0.1-project-skeleton.md
    ├── sprint1/            ← Phase 1: MVP (detailed)
    │   ├── 1.1-domain-tdd.md
    │   ├── 1.2-application-layer.md
    │   ├── 1.3-persistence.md
    │   ├── 1.4-api-endpoints.md
    │   ├── 1.5-react-shell.md
    │   ├── 1.6-categories-and-quiz.md
    │   ├── 1.7-result-and-polish.md
    │   └── 1.8-staging-deploy.md
    ├── sprint2/            ← Phase 2: Dashboard (outline only)
    ├── sprint3/            ← Phase 3: AI integration (outline only)
    └── sprint4/            ← Phase 4: Polish & deployment (outline only)
```

## Vocabulary

- **Phase** — a major milestone (Phase 0 = Foundation, Phase 1 = MVP, etc.). Defined in `docs/DECISION_LOG.md` (ADR-013).
- **Sprint folder** — `sprintN/` is the directory holding all iterations belonging to Phase N. The word "sprint" here is a folder-name convention only — no fixed time window is implied.
- **Iteration** — a single working state. Each iteration file describes one increment of work that ends in a demonstrable, deployable state. Iterations are numbered `X.Y` where X is the phase and Y is the order within that phase.

## Iteration file structure

Each iteration file follows this template:

```markdown
# Iteration X.Y — Short name

> Phase X · Status: planned | in progress | done

## Goal
One sentence: what works after this iteration.

## Definition of Done
- [ ] Concrete technical check
- [ ] Concrete technical check
- [ ] Demo-able outcome

## Tasks (in order)
1. **Task name** — short description
2. **Task name** — short description

## References
- ADR-XYZ
- mockups/screen-name.html (when applicable)
```

The format is intentionally minimal. It tells an AI assistant (or a future me) what to do, not how to do it. Implementation details are decided during the work itself, drawing on `docs/ARCHITECTURE.md`, `docs/DECISION_LOG.md`, and the mockups.

## Status tracking

Each iteration file's frontmatter shows status: `planned`, `in progress`, or `done`. When an iteration completes:

1. Update the iteration's status to `done`
2. Tick the `Definition of Done` checkboxes that were met
3. Optionally add a short "Notes" section if anything notable came up

This gives anyone opening the repo a quick read on where the project is.

## Phases 2-4

These contain `README.md` outline files only. They list planned iterations without task breakdowns. Detailed iteration files will be written when their phase becomes the current focus — premature detail risks invalidation as earlier phases reveal new information.
