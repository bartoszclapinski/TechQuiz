# Sprint 2 — Phase 2: Dashboard + Spaced Repetition

> Status: done — delivered as iterations 2.1–2.9 (see the Delivered section and `LOG.md`). The planned list below is the original pre-phase outline, kept as history.

## Phase goal
Build the Dashboard screen with bento grid analytics, introduce spaced repetition for daily review, and expose history beyond the immediate Result screen.

## Planned iterations

- **2.1 — Dashboard data layer**: API endpoints for aggregate stats (streak, score over time, category strength radar, recent activity). New use cases in Application layer, new repository methods, optional read models if perf warrants.
- **2.2 — Dashboard UI**: Build the 8-tile bento grid per `docs/mockups/dashboard-*.html`. Recharts for line + radar, inline SVG for sparkline. Empty state for first-time users.
- **2.3 — Time range filter**: Week / Month / All time filter on Dashboard. Backend filters queries accordingly. UI segmented control as designed.
- **2.4 — History page**: Full list of past attempts, filterable by category, sortable by date/score. Pagination or infinite scroll.
- **2.5 — Spaced repetition engine**: Domain logic for "Daily review" — pick questions user got wrong, weight by recency and difficulty. New endpoint `GET /api/review/daily`. Initially no UI screen, just API ready.
- **2.6 — Daily review UI**: Surface the spaced repetition queue as a card on Dashboard ("3 questions to review today"). Click → enters a quiz-like flow with mixed-category questions.

## Delivered

- **2.1–2.6** as planned — dashboard data layer (#216), dashboard UI (#221), time-range filter (#226), History page (#232), spaced-repetition engine (#239), daily review UI (#245). Spaced repetition was not deferred.
- **2.7 — Review persistence + SR feedback + stats** (unplanned; ADR-021 reverses 2.6's stateless review) — #253
- **2.8 — Review hub page** (`/review` landing, `/review/run`, session detail) — #262
- **2.9 — Achievement badges** (derive-on-read, Dashboard section; covers the badges part of the planned 4.3) — #268
- Recharts (2.2) was later removed in 4.5 (#355); the dashboard was rebuilt in the Momentum redesign (4.10, ADR-024).
- Phase 2 shipped after Phase 3 (2026-06-30 → 07-01).

## Mockups available
- `docs/mockups/dashboard-dark.html`
- `docs/mockups/dashboard-light.html`
- `docs/mockups/dashboard-empty-state.html`

## References
- ADR-013 MVP-first scope strategy
- ADR-016 Dashboard bento grid layout
- ADR-021 Review Sessions Persist and Feed the Spaced-Repetition Queue
- `docs/ARCHITECTURE.md` (Component Patterns section will gain spaced repetition specifics during this phase)

## Notes
Phase 2 transforms TechQuiz from "a quiz" into "a learning tool". The Dashboard is the **single most visually impressive screen** in the portfolio — invest time here. Spaced repetition is a stretch goal within Phase 2; if scope feels tight, defer 2.5 + 2.6 to a Phase 2.5 mini-phase.
