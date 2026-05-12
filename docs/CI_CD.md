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
5. Release event triggers deploy to Azure App Service (staging)

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

### `deploy-staging.yml` — Azure deployment

Runs after a successful `Release` workflow (`workflow_run` trigger) and on manual dispatch.

**Two jobs in parallel:**

- `deploy-api` — Builds `TechQuiz.Api`, publishes to Azure App Service (Linux, .NET 9). Then applies EF Core migrations against the staging database.
- `deploy-web` — Builds React app with `VITE_API_BASE_URL` set to staging API URL, deploys static files to a separate App Service slot.

Both jobs use the `staging` environment with manual approval gates configurable via Azure Portal (see "Environment configuration" below).

## Why Azure App Service?

This project targets .NET roles. Hosting on Azure (rather than Fly.io or Railway) communicates familiarity with Microsoft's cloud stack — directly relevant to most job postings in the niche. App Service for Linux runs .NET 9 natively, costs nothing on the F1 free tier (with limits) or ~$13/month on B1.

Trade-offs: App Service cold starts are slower than Fly.io. The free tier sleeps after 20 minutes of inactivity. Acceptable for portfolio demos; not for production traffic.

## Secrets

Configured in GitHub: **Settings → Secrets and variables → Actions**.

Required for staging deploy:

- `AZURE_CREDENTIALS` — JSON output from `az ad sp create-for-rbac`. Used by `azure/login@v2`.
- `STAGING_DB_CONNECTION` — Connection string to staging PostgreSQL (Azure Database for PostgreSQL Flexible Server).

The Azure service principal needs `Contributor` role on the resource group containing the App Service resources.

Local development uses `dotnet user-secrets` for JWT signing key and DB password — never commit secrets to the repo.

## Environment configuration

The `staging` environment in GitHub (**Settings → Environments → staging**) holds:

- Environment-specific secrets (overrides repo-level secrets if needed)
- Optional manual approval gate before each deploy
- Deployment branch policy: only `master` can deploy to staging

When production deployment is added (Phase 4), a separate `production` environment will require manual approval and pin to git tags only, not arbitrary commits.

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
