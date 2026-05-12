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

---

## 2026-05-12 — Krok 3: Solution + 7 projects + project references

**Co zrobione (4 atomic commits within `feat/project-skeleton` branch):**

1. `chore: create empty solution` — `dotnet new sln -n TechQuiz`.
2. `chore: scaffold src projects (Domain, Application, Infrastructure, Api)` — created 4 src projects targeting `net9.0`:
   - `TechQuiz.Domain` (`classlib`)
   - `TechQuiz.Application` (`classlib`)
   - `TechQuiz.Infrastructure` (`classlib`)
   - `TechQuiz.Api` (`webapi --use-controllers`)

   Removed template noise: `Class1.cs` in each classlib, `WeatherForecast.cs` + `Controllers/WeatherForecastController.cs` + `TechQuiz.Api.http` in Api. Empty `Controllers/` folder removed (will be recreated in iteration 1.4 when controllers arrive).

   Added all 4 to solution under `src/` solution folder.

3. `chore: scaffold test projects` — created 3 xunit projects under `tests/`:
   - `TechQuiz.Domain.Tests`
   - `TechQuiz.Application.Tests`
   - `TechQuiz.Infrastructure.Tests`

   Removed template `UnitTest1.cs` from each. Added to solution under `tests/` solution folder.

4. `chore: wire project references per Clean Architecture (ADR-001)` — 6 references:
   - `Application → Domain`
   - `Infrastructure → Application` (transitively → Domain)
   - `Api → Application + Infrastructure`
   - `Domain.Tests → Domain`
   - `Application.Tests → Application`
   - `Infrastructure.Tests → Infrastructure`

   Domain still has **zero project references** (correct: it's the dependency root per ADR-001).

**Decyzje:**
- **`webapi --use-controllers` (not minimal API).** CLAUDE.md and iteration 1.4 both reference controller-based design (`AuthController`, `CategoriesController`, `QuizzesController`). Keeps a thin controller layer over MediatR per ADR-006.
- **Removed all `Class1.cs` / `UnitTest1.cs` / `WeatherForecast*` placeholders.** They serve no purpose; iteration 1.1 will populate Domain with real entities via TDD.
- **`launchSettings.json` left as-is** — ports `5032`/`7145` for local `dotnet run`. Docker compose will use `8080` per iteration 0.1 task 8.
- **`appsettings.Development.json`** — not committed (`.gitignore` rule). Per project convention, sensitive dev config goes through user-secrets or env vars, not VCS.
- **`net9.0` explicit `--framework`** flag on every `dotnet new` — even though `global.json` pins to 9.0.312, the explicit framework hardens against future SDK drift.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → success, 0 warnings, 0 errors.
- `dotnet test TechQuiz.sln` → exit code 0 ("no tests available" is expected — placeholders removed, real tests come in iteration 1.1).
- `git log --oneline feat/project-skeleton ^master` → 4 commits stacked on top of `aa0acc3` (post-merge master tip).

---

## 2026-05-12 — Krok 4: EF Core + Identity packages + AppDbContext skeleton

**Co zrobione (2 commits within `feat/project-skeleton`):**

1. `chore(infra): add EF Core + Identity packages (9.x)` — 4 packages added to `TechQuiz.Infrastructure`:
   - `Microsoft.EntityFrameworkCore` 9.0.*
   - `Microsoft.EntityFrameworkCore.Design` 9.0.* (with `PrivateAssets=all` — design-time only)
   - `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.*
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.*

2. `feat(infra): add AppDbContext skeleton with Identity`:
   - `Persistence/Identity/ApplicationUser.cs` — `: IdentityUser`, empty body for now. Phase 0 doesn't need extra fields; Phase 1+ will add as needed.
   - `Persistence/AppDbContext.cs` — `: IdentityDbContext<ApplicationUser>` using **primary constructor** (`public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)`). Empty body — DbSets and configurations belong to iteration 1.3.

**Decyzje:**
- **Version constraint `9.0.*`** — `dotnet add package` without `--version` defaulted to `10.0.7` (newest on NuGet), which is `net10.0`-only and broke compatibility with our `net9.0` target. Floating `9.0.*` picks the latest 9.x patch on each restore. Acceptable for a portfolio project; if we ever want fully reproducible builds, switch to `packages.lock.json` (deferred to Phase 4 polish).
- **`ApplicationUser` in `Infrastructure`, NOT Domain.** Plan PL line "ApplicationUser : IdentityUser w Domain" would have violated ADR-001 (Domain must have zero framework deps). Identity is an Infrastructure concern; Domain operates on a `UserId` primitive, bridged via `IUserContext` (interface defined in Application in iteration 1.2).
- **Primary constructor on `AppDbContext`.** CLAUDE.md soft preference + `.editorconfig` rule `csharp_style_prefer_primary_constructors = true:suggestion`. Short, idiomatic for C# 12+.
- **No snake_case naming convention yet.** CLAUDE.md known gotcha mentions a global convention. That belongs to iteration 1.3 along with entity configurations — Phase 0 just needs the DbContext type to exist.
- **No `AddDbContext` wiring in `Program.cs` yet.** That happens with JWT + Serilog + `/health` in kroks 5–6. Krok 4 is purely Infrastructure scaffolding.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → success, 0 warnings, 0 errors.
- Project structure under `src/TechQuiz.Infrastructure/`:
  ```
  Persistence/
  ├── AppDbContext.cs
  └── Identity/
      └── ApplicationUser.cs
  ```

**Pauza — punkt wznowienia:**
- Branch: `feat/project-skeleton` (5 commits stacked on master, not yet pushed)
- Next: Krok 5 (JWT auth scaffolding in `Program.cs`) lub Krok 6 (Serilog Console + Seq). Można zrobić oba w jednej sesji wraz z `Program.cs` rewrite + `appsettings.json` + `IUserContext`/`IUnitOfWork` interface stubs jeśli zechcemy mieć DI w kompletne.
- TodoWrite snapshot przy pauzie: kroki 4 done, 5–14 pending.

---

## 2026-05-12 — Kroki 5–7: JWT + Serilog + /health (Tasks 4–6 iteracji 0.1)

**Co zrobione (4 commits within `feat/project-skeleton`):**

1. `feat(api): add JWT bearer auth scaffolding`
   - Added `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.* to Api.
   - `appsettings.json`: `Jwt: { Issuer: "TechQuiz", Audience: "TechQuiz.Client", AccessTokenLifetimeMinutes: 15, RefreshTokenLifetimeDays: 14 }`. **No signing key in repo.**
   - `Program.cs`: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` with full `TokenValidationParameters` (validate issuer/audience/lifetime/signing key, `ClockSkew = 30s`). `UseAuthentication()` placed before `UseAuthorization()`. Signing key fail-fast — `throw InvalidOperationException` at startup if missing.

2. `chore(api): initialize user-secrets for JWT signing key`
   - `dotnet user-secrets init` added `<UserSecretsId>` to `TechQuiz.Api.csproj` (the GUID is per-machine but harmless in repo).
   - Local-only: `dotnet user-secrets set "Jwt:SigningKey" "<512-bit base64>"` — 64-byte cryptographically random key generated via `RandomNumberGenerator.Create()`. Stored in `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`, not in repo.

3. `feat(api): configure Serilog with Console + Seq sinks`
   - Added `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.Seq` (latest stable).
   - `appsettings.json`: replaced default `Logging` section with `Serilog:` section. MinimumLevel `Information`, `Microsoft.AspNetCore: Warning`, `Microsoft.EntityFrameworkCore: Information`. WriteTo Console + Seq (`http://localhost:5341` default; docker compose will override via env var). Enrich `FromLogContext`. Properties `Application: "TechQuiz.Api"`.
   - `Program.cs`: `builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))` + `app.UseSerilogRequestLogging()`.

4. `feat(api): add /health with DbContext check + Infrastructure DI`
   - Added `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` 9.0.* to Api.
   - **New file** `src/TechQuiz.Infrastructure/DependencyInjection.cs` — static `AddInfrastructure(IServiceCollection, IConfiguration)` extension. Reads `ConnectionStrings:DefaultConnection`, registers `AppDbContext` with `UseNpgsql(...)`. Pattern matches iteration 1.3 task 7 — will expand with repositories, `IUserContext`, `IUnitOfWork` later.
   - `appsettings.json`: added `ConnectionStrings:DefaultConnection = Host=localhost;Port=5432;Database=techquiz;Username=techquiz;Password=techquiz_dev`. Dev defaults — same credentials docker compose will set on the postgres service.
   - `Program.cs`: `AddInfrastructure(builder.Configuration)`, `AddHealthChecks().AddDbContextCheck<AppDbContext>(name: "postgres", tags: ["db", "ready"])`, `app.MapHealthChecks("/health")`.

**Decyzje:**
- **Fail-fast on missing JWT signing key.** Throwing at startup is better than silently allowing tokens to fail validation under load. The error message tells the dev exactly how to fix it (`dotnet user-secrets set ...`).
- **`ConnectionStrings:DefaultConnection` committed with dev password.** Dev-only credentials (`techquiz_dev`), local Docker container. Same password will be set on the `postgres` service in docker-compose. Production gets a real password via env var override.
- **No `Logging` section after Serilog config.** Serilog replaces the default logger entirely — keeping the old `Logging` section would be misleading (it would be ignored). Explicit removal makes the config single-source-of-truth.
- **`Microsoft.EntityFrameworkCore: Information` log level override.** Default would be `Information`, which is fine — but explicit override pins the choice in case a transitive dep changes the default. EF Core query logging at `Information` is useful in dev for spotting N+1.
- **Health checks tags `["db", "ready"]`.** Tags allow future filtering: `MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") })`. For now we expose all checks under one `/health` endpoint.
- **`Infrastructure.DependencyInjection` referenced from Api via `using TechQuiz.Infrastructure;`.** Api → Infrastructure project reference (set in Krok 3.5) makes this work. Domain still has zero external references.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → success, 0 warnings, 0 errors.
- `dotnet run --project src/TechQuiz.Api --urls http://localhost:5032` → app starts, Serilog console output visible:
  ```
  [18:09:29 INF] Now listening on: http://localhost:5032
  [18:09:29 INF] Application started. Press Ctrl+C to shut down.
  ```
  No startup exceptions. `/health` not hit yet because postgres isn't running locally — that requires Krok 10 (docker-compose.yml) to provision the DB.

**Pauza — punkt wznowienia:**
- Branch: `feat/project-skeleton` — 12 commits stacked on master, not yet pushed.
- Next: **Krok 8 — frontend skeleton** (Vite + React + TS + Tailwind w `web/`). Wymaga zainstalowania `pnpm` najpierw (`npm install -g pnpm` lub corepack). To może być duży krok — Vite tworzy całe drzewo, dodatkowo Tailwind setup. Rozważyć podział na (a) bare Vite + TS, (b) Tailwind + design tokens.

---

## 2026-05-12 — Krok 8: Frontend skeleton (Vite + React + TypeScript + Tailwind)

**Co zrobione (3 commits within `feat/project-skeleton`):**

1. `chore(web): scaffold Vite + React + TypeScript template`
   - Installed `pnpm` 9.15.9 via `npm install -g pnpm@9` (user-scope, no UAC).
   - `npm create vite@latest web -- --template react-ts --yes` scaffolded `web/` with React 19.2.6, React DOM, Vite 8, TypeScript 6, ESLint 10, `@vitejs/plugin-react` 6.
   - `cd web && pnpm install` → `pnpm-lock.yaml` created (this is the lockfile CI expects per `.github/workflows/ci.yml`).
   - `pnpm build` smoke test → 193KB JS / 60KB gzipped, 1s build.

2. `chore(web): add Tailwind CSS + PostCSS + autoprefixer`
   - `pnpm add -D tailwindcss@^3.4.0 postcss autoprefixer` — Tailwind v3.4.19 (NOT v4: v4's CSS-first config doesn't match iteration 1.5's plan for `tailwind.config.js`).
   - `npx tailwindcss init -p` created `tailwind.config.js` + `postcss.config.js`.
   - `tailwind.config.js`: `content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}']`, `darkMode: ['class', '[data-theme="dark"]']` — matches CLAUDE.md theme strategy (data-attribute on `<html>`) + ADR-012 dual theme.
   - `src/index.css` replaced with three `@tailwind` directives.

3. `chore(web): strip Vite template demo content`
   - `App.tsx` simplified to a single centered `<h1>TechQuiz</h1>` with Tailwind utility classes.
   - Deleted: `App.css`, `src/assets/{react,vite}.svg`, `src/assets/hero.png`, `public/icons.svg`, empty `src/assets/`.
   - `index.html` title `web` → `TechQuiz`.
   - Final bundle: 190KB JS / 60KB gzipped, 3.81KB CSS / 1.44KB gzipped.

**Decyzje:**
- **pnpm via `npm install -g` (not corepack).** Corepack on this machine threw two errors: (a) `EPERM` writing to `C:\Toolchains\NodeJS\` (Node install dir requires admin), (b) "Cannot find matching keyid" — known signature verification bug with pnpm packages in older corepack. `npm install -g pnpm@9` worked cleanly to user-scope `%APPDATA%\Roaming\npm`.
- **`npm create vite` instead of `pnpm create vite`.** First two attempts via `pnpm create vite@latest web -- --template react-ts` scaffolded **vanilla-ts** (no React) despite the explicit `--template` flag. Likely pnpm arg-passing quirk with Vite 8's interactive prompts. `npm create vite@latest web -- --template react-ts --yes` worked first try.
- **`--template react-ts` (not `react-swc-ts`).** Vite 8 collapsed React templates: only `react-ts` and `react-compiler-ts` exist now (verified via `create-vite --help`). SWC is no longer the differentiator — `@vitejs/plugin-react` is Babel-based but Vite 8's HMR is fast enough. Trade-off acknowledged: `react-compiler-ts` would use the React 19 compiler (auto-memoization) which could be a portfolio talking point — defer the decision to iteration 1.5 polish.
- **Tailwind v3.4 over v4.** v4 (stable Jan 2025) uses CSS-first config without `tailwind.config.js`. Iteration 1.5 explicitly plans `tailwind.config.js` for design tokens. v3 has more documentation and fits the planned workflow. v4 migration possible in Phase 4 polish if needed.
- **`darkMode: ['class', '[data-theme="dark"]']` in tailwind.config.js.** Matches CLAUDE.md's known gotcha — dual theme via `data-theme` attribute on `<html>`. Both selectors active simultaneously: `dark:` Tailwind variants fire when either `.dark` class or `[data-theme="dark"]` is present on an ancestor. Future `ThemeProvider` (iteration 1.5) toggles via `data-theme` attribute.
- **No design tokens yet.** Iteration 1.5 task 1 will extend `theme` with custom violet shades + slate scale + Geist + JetBrains Mono. Krok 8 leaves the config minimal — only content paths and dark mode.
- **Demo content fully removed**, not preserved as reference. Mockups in `docs/mockups/*.html` are the visual reference; keeping Vite demo would be noise.

**Weryfikacja:**
- `pnpm build` → success, 1.00s, 16 modules transformed, no errors.
- `pnpm-lock.yaml` committed (CI uses `--frozen-lockfile`).
- Project structure under `web/`:
  ```
  web/
  ├── eslint.config.js
  ├── index.html
  ├── package.json
  ├── pnpm-lock.yaml
  ├── postcss.config.js
  ├── tailwind.config.js
  ├── public/favicon.svg
  ├── src/
  │   ├── App.tsx        (TechQuiz heading + Tailwind utilities)
  │   ├── index.css      (3 @tailwind directives)
  │   └── main.tsx
  ├── tsconfig.app.json
  ├── tsconfig.json
  ├── tsconfig.node.json
  └── vite.config.ts
  ```

**Pauza — punkt wznowienia:**
- Branch: `feat/project-skeleton` — 17 commits stacked on master, not pushed yet.
- Next: **Krok 9 — Dockerfile API**. `mcr.microsoft.com/dotnet/sdk:9.0` for build, `mcr.microsoft.com/dotnet/aspnet:9.0` for runtime. Multi-stage. Exposes 8080. Iteration 0.1 tasks 8–10 (Dockerfiles + docker-compose) form a natural group.
