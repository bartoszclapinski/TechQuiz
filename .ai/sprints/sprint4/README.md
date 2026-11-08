# Sprint 4 — Phase 4: Polish & Deployment

> Status: done — delivered as iterations 4.1, 4.2, 4.4–4.11 (no separate 4.3; see the Delivered section and `LOG.md`). The planned list below is the original pre-phase outline, kept as history.

## Phase goal
Production deployment, mobile responsive UI, gamification (XP, achievements, leveling), accessibility audit, performance optimization.

## Planned iterations

- **4.1 — Mobile responsive**: Adapt all screens for mobile breakpoints (375px-768px). Topbar becomes hamburger menu, dashboard collapses to single column with reordered tiles, quiz options grow tap targets.
- **4.2 — Gamification — XP and levels**: Domain logic for XP awards per correct answer, difficulty multipliers, level thresholds. New UI: level badge in topbar, XP gain animation in Result screen.
- **4.3 — Gamification — achievements**: Unlockable badges for milestones ("First 100%", "7-day streak", "Mastered a category"). Toast notifications when unlocked. Achievement gallery page.
- **4.4 — Accessibility audit**: Full WAVE + axe + manual screen reader pass. Fix all critical issues. Document a11y status in README.
- **4.5 — Performance optimization**: Lighthouse perf to 90+. Bundle analysis, code splitting per route, image lazy loading, font preload, Service Worker for offline read-only.
- **4.6 — Production deployment**: Choose host (Azure, Railway, Render, Fly.io). CI/CD pipeline pushes to staging on master, manual promote to production. Domain, SSL, monitoring (Application Insights or Sentry).
- **4.7 — Final portfolio polish**: Update README with production link, video walkthrough, blog post linking to repo, LinkedIn announcement.

## Delivered

- **4.1** — Mobile responsive (#273).
- **4.2** — Gamification: XP / levels / Skill IQ (#337–#343, ADR-025). Delivered differently from the plan: flat 10 XP per correct answer (no difficulty multiplier), derived on read; level + XP bar and Skill IQ on the Dashboard hero; a static "XP earned" tile on Result; no topbar badge or XP animation.
- **4.3 — not run as a separate iteration.** Achievement badges shipped early in **2.9** (#268) as a Dashboard section. The unlock toasts and the achievement gallery page were **not built**.
- **4.4** — Accessibility to WCAG 2.1 AA: focus ring + skip link (#345), contrast (#347, ADR-026), semantics/ARIA (#349). Delivered as a code/component audit + computed-contrast pass; no WAVE/axe/screen-reader run is recorded, and the README carries a one-line a11y summary rather than a status section.
- **4.5** — Route-based code splitting (#353) + dropped unused recharts (#355). The Lighthouse-90 target, image lazy-loading, font preload and offline Service Worker were not pursued.
- **4.6** — Host chosen: **Render + Neon** (ADR-022). Single live environment auto-deployed by Render on push to `master` (`render.yaml`); no staging→production promotion, no custom domain (Render-managed `*.onrender.com` TLS), no Sentry/App Insights — health is checked by `.github/workflows/api-smoke.yml`. Delivered in #284, #286, #288, #290.
- **4.7** — SEO/social meta (#359), README refresh with live URL + demo credentials (#361), a11y tail polish (#363). The walkthrough GIF/video followed in #368. Blog post / LinkedIn announcement were not tracked in the repo.
- Added during the phase: **4.8** Category taxonomy — Tracks (#300, ADR-023); **4.9** Desktop typography scale (#303); **4.10** Momentum redesign (#305–#321, ADR-024); **4.11** Demo hardening — seeded demo history + closed registration (#331, #333).

## Mockups available
No static mockups in `docs/mockups/` for Phase 4. Mobile variants were designed iteratively in 4.1; the Momentum redesign (4.10, ADR-024) was built from the design handoff in `.ai/design/momentum-handoff/`.

## References
- ADR-013 MVP-first scope strategy
- ADR-017 CI/CD Strategy (staging deploy target amended by ADR-022)
- ADR-022 Staging Host — Render + Neon instead of Azure App Service
- ADR-023 Two-Level Category Taxonomy — Tracks over Categories
- ADR-024 "Momentum" Visual Redesign — Violet + Amber Design System
- ADR-025 Gamification — XP, Levels, and Skill IQ Derived from Existing Attempts
- ADR-026 Accessibility Contrast Adjustments to Three Momentum Tokens

## Notes
**Phase 4 is when the project becomes a portfolio piece, not just a learning project.** Until 4.6 ships a live URL, visitors can only see screenshots and code. After 4.6, they can click and try it.

Gamification (4.2-4.3) is **optional**. It's a "fun factor" that lifts the project from "another quiz CRUD" into "engaging product", but if scope tightens, defer to a Phase 5 or skip entirely. The deployment and accessibility work is **not optional** — without those, the portfolio piece is incomplete.
