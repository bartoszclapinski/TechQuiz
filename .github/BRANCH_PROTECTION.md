# Branch Protection Setup

GitHub doesn't store branch protection rules in the repository — they're configured per-repo via the web UI. This document captures the protection settings for `master`, so anyone (including future-me) can reproduce them on a fresh repo or recover from accidental changes.

## Protection rules for `master`

Navigate to: **Settings → Branches → Branch protection rules → Add rule**

**Branch name pattern:** `master`

### Required settings

- ☑ **Require a pull request before merging**
  - ☐ Require approvals (left **off** — solo project, no second reviewer)
  - ☑ Dismiss stale pull request approvals when new commits are pushed (no-op while approvals are off, but enables it cleanly later)

- ☑ **Require status checks to pass before merging**
  - ☑ Require branches to be up to date before merging
  - Required checks (add these names exactly as they appear in CI):
    - `Backend (build + test)`
    - `Frontend (build + lint)`
    - `Commitlint (PR title + commits)`

- ☑ **Require conversation resolution before merging**

- ☑ **Require linear history** (forces squash or rebase merge — no merge commits on `master`)

- ☐ Require signed commits (off — adds friction, no security benefit for solo project)

- ☐ Require deployments to succeed (off — deploys run after merge, not before)

### Rules NOT enabled

- ❌ Allow force pushes (kept off — no rewriting `master` history)
- ❌ Allow deletions (kept off — `master` is permanent)

### Repository-wide settings

In **Settings → General → Pull Requests**:
- ☐ Allow merge commits (off)
- ☑ Allow squash merging
  - Default to "Pull request title and description" for commit messages
- ☐ Allow rebase merging (off — keep history simple)
- ☑ Always suggest updating pull request branches
- ☑ Automatically delete head branches after merge

## Why these settings?

- **Squash-only merges** keep `master` history linear and each PR appears as one commit (whose message follows Conventional Commits). This is what semantic-release reads to decide version bumps.
- **No required reviewers** because this is a solo project. CI is the gatekeeper.
- **Required CI checks** mean `master` is always green: every commit on `master` represents a passing build.
- **Linear history + auto-delete branches** keeps the repo clean as iterations stack up.

## When working with Claude Code

PR-based workflow still applies. Even when an AI assistant writes the code, the flow is:

1. Assistant works on a feature branch
2. Assistant (or you) opens a PR via `gh pr create`
3. CI runs
4. You merge via GitHub UI (squash) after CI passes

Direct push to `master` is blocked. This is intentional — it forces CI on every change, including AI-generated ones.
