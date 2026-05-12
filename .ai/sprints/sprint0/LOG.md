# Sprint 0 — Session Log

> Chronological record of work done in Sprint 0 (Phase 0 — Foundation).
> Each entry: date, what was done, decisions made, verification result.
> See `0.1-project-skeleton.md` for the iteration plan and Definition of Done.

---

## 2026-05-12 — Krok 1: Initial repo skeleton + GitHub repo

**Co zrobione:**
- Verified environment: `.NET 10 SDK installed (10.0.201), .NET 9 SDK also present (9.0.312, 9.0.116). Node 22.12.0. Docker 28.5.1 + Compose 2.40. pnpm missing (deferred to frontend step). git 2.46.`
- Created `global.json` pinning SDK to `9.0.312` with `rollForward: latestFeature`.
- `git init -b main` in `f:\Repos\TechQuiz\`.
- Discovered substantial pre-existing scaffolding from earlier sessions:
  - Full documentation tree: `CLAUDE.md`, `README.md`, `docs/ARCHITECTURE.md`, `docs/DECISION_LOG.md`, `docs/CI_CD.md`, `docs/mockups/*.html`.
  - `.ai/` planning: `ONBOARDING.md`, `QUICK_START.md`, all sprint iteration files.
  - GitHub config: `.github/workflows/{ci,release,deploy-staging}.yml`, `BRANCH_PROTECTION.md`, `PULL_REQUEST_TEMPLATE.md`.
  - Repo config: `.gitignore`, `.releaserc.json`, `commitlint.config.cjs`.
- Added `.claude/settings.local.json` to `.gitignore` (Claude Code user-specific permissions).
- Initial commit `ef90528` — 43 files, 5392 insertions.
- Created GitHub repo `bartoszclapinski/TechQuiz` (private) via `gh repo create --source=. --remote=origin --push`.
- Set repo description: *"Portfolio project: technical-knowledge quiz platform. Clean Architecture (.NET 9) + React/TypeScript, multi-provider AI, PostgreSQL, full CI/CD."*
- Added 12 topics: `dotnet`, `aspnetcore`, `csharp`, `clean-architecture`, `entity-framework-core`, `postgresql`, `react`, `typescript`, `tailwindcss`, `tdd`, `mediatr`, `portfolio`.

**Decyzje:**
- **SDK pin to 9.0.312.** Default SDK on the machine is 10.0.201 — `global.json` forces .NET 9 to match CLAUDE.md target. `rollForward: latestFeature` allows newer 9.0.x patches without editing.
- **Single initial commit covering all pre-existing files.** They form a coherent baseline (documentation + infra config). Splitting them into multiple commits would only inflate noise.
- **`docs/mockups/login-dual-theme.html` vs `login-{dark,light}.html`.** Filename mismatch with CLAUDE.md mockup reference list — left for later iteration (UI work in 1.5), not blocking now.
- **Private repo.** Per `.ai/TechQuiz_Plan_Roboczy_PL.md`: private until Phase 1 done, then flip public.

**Weryfikacja:**
- `git log --oneline` → `ef90528 chore: initial repo skeleton`
- `gh repo view` → name=TechQuiz, visibility=PRIVATE, defaultBranch=main, 12 topics confirmed
- Remote: https://github.com/bartoszclapinski/TechQuiz

---

## 2026-05-12 — PR #1: Rename default branch `main` → `master`

**Co zrobione:**
- Server-side rename via `gh api -X POST repos/bartoszclapinski/TechQuiz/branches/main/rename -f new_name=master` (atomically renames branch + redirects default-branch refs on GitHub, no PR loss).
- Local sync: `git fetch --prune`, `git branch -m main master`, `git branch -u origin/master master`, `git remote set-head origin -a`.
- Updated 15 files via feature branch `chore/rename-default-to-master`:
  - Workflows + config: `ci.yml`, `release.yml`, `deploy-staging.yml`, `.releaserc.json` (CI triggers would not fire without this).
  - Docs: `BRANCH_PROTECTION.md`, `CLAUDE.md`, `README.md`, `docs/{ARCHITECTURE,CI_CD,DECISION_LOG}.md`.
  - `.ai/`: `ONBOARDING.md`, `TechQuiz_Plan_Roboczy_PL.md`, `sprints/sprint0/0.1-project-skeleton.md`, `sprints/sprint1/1.8-staging-deploy.md`, `sprints/sprint4/README.md`.
- Updated `ONBOARDING.md` policy line (was: *"commit directly to main is acceptable for trivial changes"*) → new policy: **never push directly to master, always feature branch + PR**.
- PR #1 squash-merged → commit `9887a17`. Branch `chore/rename-default-to-master` auto-deleted.

**Decyzje:**
- **`master` as default branch.** Owner's explicit preference (2026-05-12), conscious choice over the modern `main` convention.
- **One PR for all 15 file edits.** Single conceptual change (rename); splitting by file group would inflate PR count for no readability gain.
- **Skipped renaming HTML `<main>` tags, `main.tsx` filename, idiomatic "main CTA"** — those are not branch references.
- **Branch protection NOT enabled server-side.** Both classic branch-protection API and rulesets API returned HTTP 403 — both require GitHub Pro on private repos. Discipline + manual verification is the only enforcement until either: (a) repo goes public after Phase 1, or (b) owner upgrades to Pro. Documented in memory `feedback_branch_discipline.md`.

**Weryfikacja:**
- `git log --oneline` → `9887a17`, `ef90528`
- `gh pr view 1` → MERGED
- `grep -rn '\bmain\b'` → only HTML `<main>` tags, `main.tsx`, "main CTA" idiom remain (no branch refs).

---

## 2026-05-12 — Krok 2: `.editorconfig` + sprint LOG

**Co zrobione:**
- Created `.editorconfig` at repo root with sections for: defaults (UTF-8, LF, trim trailing, insert final newline), C# (file-scoped namespaces, Allman braces, `_camelCase` private fields, modern language preferences), config files / frontend / Markdown / Makefile.
- Created this `LOG.md` with three retroactive entries covering Krok 1, PR #1 rename, and Krok 2.

**Decyzje:**
- **`end_of_line = lf`** — Windows git autocrlf handles the LF↔CRLF dance at checkout. Repo stores LF (visible in earlier `git add` warnings).
- **`csharp_prefer_braces = true:suggestion`** (not warning) — leaves room for deliberate one-line `if`s without ceremony.
- **`csharp_style_var_*` settings** match CLAUDE.md soft preference: `var` for built-ins and apparent types, **explicit type elsewhere on public API surface**.
- **`_camelCase` for private/internal fields** matches `.ai/QUICK_START.md` naming convention.
- **`csharp_style_prefer_primary_constructors = true:suggestion`** — CLAUDE.md mentions "primary constructors where they help" — suggestion (not warning) keeps it as a hint.
- **`trim_trailing_whitespace = false` for Markdown** — required because Markdown uses trailing double-space as a hard line break.

**Weryfikacja:** pending — commit + push + PR + squash merge to follow this write.
