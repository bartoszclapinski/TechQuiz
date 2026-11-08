# `.ai/` — Operational documents for AI assistants

This folder contains working documents intended for AI coding assistants (Claude Code, Copilot, Cursor) and for the project owner. Unlike `docs/`, which is portfolio-facing and describes the project at a high level, `.ai/` is **operational** — it records how every iteration was planned and delivered (goal, Definition of Done, tasks, and a per-sprint `LOG.md`). Phases 0–4 are complete; Phase 5 (post-launch, ADR-027) is in progress in `sprints/sprint5/`. For work outside it, agree the goal with the owner and write a new iteration file first (see `CLAUDE.md`).

## Why is this folder public?

This project is built in active collaboration with AI assistants. The `.ai/` folder exists as a deliberate, visible part of the workflow rather than something hidden. It demonstrates how the codebase is planned and how implementation work is structured.

If you're an AI assistant reading this: start with `CLAUDE.md` at the repository root, then come back here.

## Structure

```
.ai/
├── README.md               ← you are here
├── ONBOARDING.md           ← longer project background for a new session
├── QUICK_START.md          ← one-page orientation
├── animation/              ← walkthrough scene source + render scripts (README GIF, landing video)
├── design/                 ← design references incl. the Momentum handoff (ADR-024)
└── sprints/
    ├── sprint0/            ← Phase 0: Foundation — 0.1 + LOG.md
    ├── sprint1/            ← Phase 1: MVP — 1.1–1.8 + LOG.md
    ├── sprint2/            ← Phase 2: Dashboard + spaced repetition — README (outline) + 2.1–2.9 + LOG.md
    ├── sprint3/            ← Phase 3: AI integration + code challenges — README (outline) + 3.1–3.7 + LOG.md
    ├── sprint4/            ← Phase 4: Polish & deployment — README (outline) + 4.1, 4.2, 4.4–4.11 + LOG.md
    └── sprint5/            ← Phase 5: Post-launch (ADR-027) — README (outline) + 5.1 + LOG.md
```

Iteration files are named `X.Y-short-name.md` (e.g. `sprint1/1.3-persistence.md`). There is no `4.3` file: the badges part of its achievements scope shipped early in `2.9`, and the rest (gallery page, unlock toasts) was not built.

## Vocabulary

- **Phase** — a major milestone (Phase 0 = Foundation, Phase 1 = MVP, etc.). Defined in `docs/DECISION_LOG.md` (ADR-013).
- **Sprint folder** — `sprintN/` is the directory holding all iterations belonging to Phase N. The word "sprint" here is a folder-name convention only — no fixed time window is implied.
- **Iteration** — a single working state. Each iteration file describes one increment of work that ends in a demonstrable, deployable state. Iterations are numbered `X.Y` where X is the phase and Y is the order within that phase.

## Iteration file structure

Each iteration file follows this template:

```markdown
# Iteration X.Y — Short name

> Phase X · Status: planned | in-progress | done

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
- docs/mockups/screen-name.html (when applicable)
```

The format is intentionally minimal. It tells an AI assistant (or a future me) what to do, not how to do it. Implementation details are decided during the work itself, drawing on `docs/ARCHITECTURE.md`, `docs/DECISION_LOG.md`, and the mockups.

## Status tracking

Each iteration file shows its status near the top, in one of two formats:

- a `> Phase X · Status: …` line under the title (the template above — used by most files), or
- a YAML frontmatter `status:` field (used by the later Phase 4 files: 4.2, 4.4, 4.5, 4.7, 4.10, 4.11).

Values: `planned`, `in-progress`, `done`. A DoD item replaced by other work is struck through and marked `superseded by …` rather than ticked. The phase outlines (`sprint2/3/4/README.md`) carry their own `> Status:` line.

When an iteration completes:

1. Update the iteration's status to `done`
2. Tick the `Definition of Done` checkboxes that were met
3. Optionally add a short "Notes" section if anything notable came up
4. Add an entry to the sprint's `LOG.md` (Polish, newest entry first — the sprint0, sprint1 and sprint3 logs predate that convention and run oldest-first)

This gives anyone opening the repo a quick read on where the project is.

## Phases 2-4

Each `sprintN/README.md` is the **original outline** for its phase, written before work started. Every planned phase has since been delivered, and each folder now holds detailed iteration files plus a `LOG.md`. Each outline keeps its planned list as history and adds a **Delivered** section mapping plan → actual iterations. Treat the iteration files and `LOG.md`, not the outline, as the record of what shipped.
