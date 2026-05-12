# Sprint 4 — Phase 4: Polish & Deployment

> Status: outlined (detail expansion deferred until current phase completes)

## Phase goal
Production deployment, mobile responsive UI, gamification (XP, achievements, leveling), accessibility audit, performance optimization.

## Planned iterations

- **4.1 — Mobile responsive**: Adapt all screens for mobile breakpoints (375px-768px). Topbar becomes hamburger menu, dashboard collapses to single column with reordered tiles, quiz options grow tap targets.
- **4.2 — Gamification — XP and levels**: Domain logic for XP awards per correct answer, difficulty multipliers, level thresholds. New UI: level badge in topbar, XP gain animation in Result screen.
- **4.3 — Gamification — achievements**: Unlockable badges for milestones ("First 100%", "7-day streak", "Mastered a category"). Toast notifications when unlocked. Achievement gallery page.
- **4.4 — Accessibility audit**: Full WAVE + axe + manual screen reader pass. Fix all critical issues. Document a11y status in README.
- **4.5 — Performance optimization**: Lighthouse perf to 90+. Bundle analysis, code splitting per route, image lazy loading, font preload, Service Worker for offline read-only.
- **4.6 — Production deployment**: Choose host (Azure, Railway, Render, Fly.io). CI/CD pipeline pushes to staging on main, manual promote to production. Domain, SSL, monitoring (Application Insights or Sentry).
- **4.7 — Final portfolio polish**: Update README with production link, video walkthrough, blog post linking to repo, LinkedIn announcement.

## Mockups available
None for Phase 4. Mobile responsive variants designed during 4.1 iteratively.

## References
- ADR-005 Docker + docker-compose (transitions to production deployment in 4.6)
- ADR-013 MVP-first scope strategy

## Notes
**Phase 4 is when the project becomes a portfolio piece, not just a learning project.** Until 4.6 ships a live URL, recruiters can only see screenshots and code. After 4.6, they can click and try it.

Gamification (4.2-4.3) is **optional**. It's a "fun factor" that lifts the project from "another quiz CRUD" into "engaging product", but if scope tightens, defer to a Phase 5 or skip entirely. The deployment and accessibility work is **not optional** — without those, the portfolio piece is incomplete.
