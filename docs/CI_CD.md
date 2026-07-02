# CI/CD

This document describes the continuous integration and deployment setup for TechQuiz.

## Overview

```
┌──────────────┐   ┌────────────┐   ┌─────────────┐   ┌──────────────┐
│ Feature      │ → │ PR to master │ → │ Squash    │ → │ Auto release │
│ branch       │   │ + CI green │   │ merge       │   │ + deploy     │
└──────────────┘   └────────────┘   └─────────────┘   └──────────────┘
```

1. Work happens on feature branches (`feat/auth-flow`, `fix/quiz-progress-bar`)
2. PR to `master` triggers CI (build + test + lint + commitlint)
3. After CI passes, the PR can be squash-merged
4. Merge to `master` triggers semantic-release → version bump → tag → CHANGELOG update
5. Render rebuilds and redeploys the staging services on the push to `master` (see Deployment below)

## Workflows

### `ci.yml` — Continuous Integration

Runs on every PR to `master` and every push to `master`.

**Jobs:**

| Job | Tooling | What it checks |
|---|---|---|
| `Backend (build + test)` | .NET 9, PostgreSQL 16 service | `dotnet build`, `dotnet test` with real Postgres |
| `Frontend (build + lint)` | Node.js 20, pnpm 9 | `pnpm lint`, `pnpm build` |
| `Commitlint (PR title + commits)` | commitlint v19, conventional-config | PR title + every commit in PR follow Conventional Commits |

All three jobs run in parallel. PR cannot merge until all three are green (enforced via branch protection — see `.github/BRANCH_PROTECTION.md`).

The backend job uses GitHub Actions service containers for PostgreSQL so integration tests run against a real database, matching production behavior.

### `release.yml` — Semantic Release

Runs on push to `master` (i.e., after a PR is merged).

Reads commit messages on `master` since last release. Decides version bump based on Conventional Commit types:

- `feat:` → minor bump (e.g., `1.2.0` → `1.3.0`)
- `fix:`, `perf:`, `refactor:` → patch bump (e.g., `1.2.0` → `1.2.1`)
- `feat!:` or footer `BREAKING CHANGE:` → major bump (e.g., `1.2.0` → `2.0.0`)
- `docs:`, `style:`, `test:`, `chore:`, `ci:`, `build:` → no release

On a successful release, the workflow:
1. Updates `CHANGELOG.md`
2. Creates a Git tag `vX.Y.Z`
3. Creates a GitHub Release with auto-generated notes
4. Pushes a `chore(release): vX.Y.Z` commit back to `master` (the only commit on `master` not from a PR — recognised by branch protection because the GitHub Actions bot has bypass)

Configuration: `.releaserc.json` at repo root.

## Deployment — Render + Neon

Deployment is **not** a GitHub Actions workflow. Staging runs on **Render**, driven by the
[`render.yaml`](../render.yaml) Blueprint at the repo root: two Docker web services built from the repo's
own Dockerfiles (the `.NET` API and the nginx-served SPA), with **Neon** as the managed PostgreSQL
provider. Render **auto-builds and redeploys on every push to `master`** (`autoDeploy: true`), so a merge
that CI has already gated flows straight to staging.

The full step-by-step (provisioning, secrets, verification) lives in
[`docs/DEPLOYMENT.md`](DEPLOYMENT.md). The choice of Render over the earlier Azure App Service plan — and
why — is recorded in **ADR-022** (student Azure credit is finite; Azure's free tier can't run our
containers).

- **API** — Render builds `src/TechQuiz.Api/Dockerfile`, binds Render's `$PORT`, runs `ASPNETCORE_ENVIRONMENT=Staging`, applies EF Core migrations on startup, and seeds the demo data (idempotent).
- **Web** — Render builds `web/Dockerfile` (nginx, SPA fallback), baking `VITE_API_BASE_URL` at build time so the SPA calls the deployed API.

### Secrets

Deploy secrets live in **Render's dashboard** (per service), never in the repo (declared `sync: false`
in `render.yaml`):

- `ConnectionStrings__DefaultConnection` — the Neon PostgreSQL connection string.
- `Jwt__SigningKey` — a strong random signing key (`openssl rand -base64 48`).

Non-secret config (environment name, CORS origin, API URL) is committed in `render.yaml`. Local
development still uses `dotnet user-secrets` for the JWT key and DB password — never commit secrets.

### Cold starts

Render's free tier sleeps a service after ~15 minutes idle; the next request wakes it (~30–50 s). Neon's
free compute autosuspends similarly. Acceptable for a portfolio demo; the README calls it out.

When a dedicated **production** environment is added later, it reuses the same Render Blueprint shape with
its own services + secrets.

## Versioning policy

- Pre-1.0: this project starts on `0.1.0` after the first feature merge. Anything before that is `0.0.0` (no release yet).
- `1.0.0` is cut after Phase 1 (MVP) completes — first version with a complete user-facing feature set.
- `2.0.0` is reserved for breaking changes to the public API or data model migrations that can't be backward-compatible.

The CHANGELOG and Git tags are the single source of truth for what's in each version.

## Commitlint configuration

Conventional Commits enforced both locally (via husky `commit-msg` hook) and in CI (`commitlint` job on PRs).

Allowed types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`.

No scope required, but encouraged for clarity (`feat(quiz): add keyboard shortcuts`, `fix(auth): refresh token rotation`).

## Local pre-flight

Before pushing a PR, run locally:

```bash
# Backend
dotnet build TechQuiz.sln
dotnet test TechQuiz.sln

# Frontend
cd web
pnpm lint
pnpm build
```

If these pass, CI will pass. The CI configuration is intentionally identical to local commands — no surprises.
