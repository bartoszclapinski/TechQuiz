# Deployment runbook — staging on Render + Neon

Staging is hosted on **Render** (two Docker web services, built from the repo's Dockerfiles) with
**Neon** as the managed PostgreSQL provider. The decision and rationale are recorded in **ADR-022**;
this document is the step-by-step runbook.

Everything Render needs is declared in [`render.yaml`](../render.yaml) (a Blueprint). Render auto-builds
and redeploys both services on every push to `master`.

```
 techquiz-web (nginx SPA)  ──fetch──►  techquiz-api (.NET)  ──conn string──►  Neon (PostgreSQL)
 *.onrender.com                        *.onrender.com                          *.neon.tech
```

---

## One-time setup

### 1. Neon — the database

1. Create a free account at <https://neon.tech> and a new project (region close to the Render region —
   **EU / Frankfurt**).
2. Neon creates a database and shows a **connection string**. Copy the **`.NET`/psql** form; it looks
   like:
   ```
   Host=ep-xxx.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=...;SSL Mode=Require;Trust Server Certificate=true
   ```
   > If Neon only shows a `postgres://…` URL, convert it to the Npgsql key/value form above (Host /
   > Database / Username / Password), and keep `SSL Mode=Require;Trust Server Certificate=true` — Neon
   > requires TLS.
3. Keep this string for step 2.4. **Never commit it.**

### 2. Render — the services

1. Create a free account at <https://render.com> and connect your GitHub (authorize the `TechQuiz` repo).
2. **New → Blueprint** → pick the `TechQuiz` repo. Render reads `render.yaml` and proposes two services:
   `techquiz-api` and `techquiz-web`. Apply.
3. The first build of `techquiz-api` will start but **fail health checks until secrets are set** — that's
   expected. Open **techquiz-api → Environment** and set the two `sync: false` secrets:
   - `ConnectionStrings__DefaultConnection` → the Neon string from step 1.2
   - `Jwt__SigningKey` → a strong random value, e.g. generate one with:
     ```
     openssl rand -base64 48
     ```
4. (Already in `render.yaml`, no action needed, but verify:) `techquiz-api` has
   `ASPNETCORE_ENVIRONMENT=Staging` and `Cors__AllowedOrigins__0=https://techquiz-web.onrender.com`;
   `techquiz-web` has `VITE_API_BASE_URL=https://techquiz-api.onrender.com`.
5. Trigger a redeploy of `techquiz-api` (**Manual Deploy → Deploy latest commit**) now that secrets exist.

> **URL note.** `render.yaml` assumes the service URLs are `techquiz-api.onrender.com` and
> `techquiz-web.onrender.com`. Render usually honours the service name, but if it appends a suffix
> (e.g. `techquiz-api-xyz.onrender.com`), update the two cross-referencing env vars
> (`Cors__AllowedOrigins__0` and `VITE_API_BASE_URL`) to the real URLs and redeploy. These two must
> match the actual hostnames or CORS / the refresh cookie will fail.

---

## How migrations run

The API applies EF Core migrations **on startup** in Staging (the same host-boots-then-migrates path the
container already uses). No separate migration step is needed on Render. The `Staging` environment also
**seeds** the demo user + questions (idempotent — a no-op once the DB has data), so the live URL is
demo-able immediately. Demo login: `demo@techquiz.local` / `DemoPass123!`.

## How a deploy happens

- **Automatic:** push to `master` → Render rebuilds and redeploys both services (`autoDeploy: true`).
- **Manual:** Render dashboard → service → **Manual Deploy**.

## Verifying a deploy

1. **API health:** `https://techquiz-api.onrender.com/health` → `200` (`Healthy`).
2. **Web loads:** open `https://techquiz-web.onrender.com` — the login screen renders.
3. **Deep-link/SPA fallback:** hard-refresh `https://techquiz-web.onrender.com/dashboard` → the app
   loads (not a 404). Confirms the nginx SPA fallback shipped.
4. **Full auth path (the important one):** log in as the demo user → take a quiz → see the result →
   dashboard renders. Then **leave the tab open ~a minute and keep navigating** to confirm the session
   **refresh** works — this exercises the cross-site `SameSite=None` refresh cookie (the most likely
   thing to break in a cross-origin deploy).

## Known behaviour

- **Cold starts.** Render's free tier sleeps a service after ~15 minutes idle; the next request wakes it
  and can take ~30–50 seconds. This is acceptable for a portfolio demo — the README calls it out so
  reviewers aren't surprised.
- **Neon autosuspend.** Neon's free compute also suspends when idle and wakes on the next connection
  (adds a second or two to the first query after a pause).

## Secrets checklist (never committed — hard rule #1)

| Where | Key | Value |
|---|---|---|
| Render · techquiz-api | `ConnectionStrings__DefaultConnection` | Neon connection string |
| Render · techquiz-api | `Jwt__SigningKey` | `openssl rand -base64 48` |

Everything else (environment name, CORS origin, API URL) is non-secret and lives in `render.yaml`.

## Troubleshooting

- **API deploy is unhealthy / restarts.** Check the Render logs. Most common: missing/invalid
  `ConnectionStrings__DefaultConnection` (API can't reach Neon) or a Neon string without
  `SSL Mode=Require`.
- **Login works but I get logged out / 401 on refresh.** The refresh cookie isn't coming back — verify
  the web and API URLs match `Cors__AllowedOrigins__0` / `VITE_API_BASE_URL` exactly (scheme + host), and
  that both are HTTPS (required for `SameSite=None; Secure`).
- **CORS error in the browser console.** `Cors__AllowedOrigins__0` doesn't match the web origin; fix and
  redeploy the API.
- **Port binding / health check fails.** The API honours Render's `PORT` (see `Program.cs`) and nginx
  listens on `${PORT}` (see `web/Dockerfile`), so this should just work; if not, confirm Render didn't
  set an unexpected port in the logs.
