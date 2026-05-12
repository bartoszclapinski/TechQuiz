# Sprint 2 — Phase 2: Dashboard + Spaced Repetition

> Status: outlined (detail expansion deferred until current phase completes)

## Phase goal
Build the Dashboard screen with bento grid analytics, introduce spaced repetition for daily review, and expose history beyond the immediate Result screen.

## Planned iterations

- **2.1 — Dashboard data layer**: API endpoints for aggregate stats (streak, score over time, category strength radar, recent activity). New use cases in Application layer, new repository methods, optional read models if perf warrants.
- **2.2 — Dashboard UI**: Build the 8-tile bento grid per `mockups/dashboard-*.html`. Recharts for line + radar, inline SVG for sparkline. Empty state for first-time users.
- **2.3 — Time range filter**: Week / Month / All time filter on Dashboard. Backend filters queries accordingly. UI segmented control as designed.
- **2.4 — History page**: Full list of past attempts, filterable by category, sortable by date/score. Pagination or infinite scroll.
- **2.5 — Spaced repetition engine**: Domain logic for "Daily review" — pick questions user got wrong, weight by recency and difficulty. New endpoint `GET /api/review/daily`. Initially no UI screen, just API ready.
- **2.6 — Daily review UI**: Surface the spaced repetition queue as a card on Dashboard ("3 questions to review today"). Click → enters a quiz-like flow with mixed-category questions.

## Mockups available
- `mockups/dashboard-dark.html`
- `mockups/dashboard-light.html`
- `mockups/dashboard-empty-state.html`

## References
- ADR-013 MVP-first scope strategy
- ADR-016 Dashboard bento grid layout
- `docs/ARCHITECTURE.md` (Component Patterns section will gain spaced repetition specifics during this phase)

## Notes
Phase 2 transforms TechQuiz from "a quiz" into "a learning tool". The Dashboard is the **single most visually impressive screen** in the portfolio — invest time here. Spaced repetition is a stretch goal within Phase 2; if scope feels tight, defer 2.5 + 2.6 to a Phase 2.5 mini-phase.
