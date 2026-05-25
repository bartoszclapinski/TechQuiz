# TechQuiz API — Postman collection

Auth endpoints delivered in iteration 1.4 session A. Quiz endpoints arrive in session 1.4-B.

## Files

- `TechQuiz.postman_collection.json` — three POSTs under an `Auth/` folder: register, login, refresh.
- `TechQuiz.local.postman_environment.json` — `baseUrl`, `accessToken`, `refreshToken` for the local docker-compose stack.

Login + refresh include post-response scripts that stash `accessToken` and `refreshToken` into the active Postman environment, so quiz endpoints in 1.4-B will pick them up automatically without copy-paste.

## Prerequisites

1. Containers up:
   ```
   docker compose up -d
   ```
   Brings up Postgres on `127.0.0.1:5433` and the API on `127.0.0.1:8080`.

2. Migrations applied. `docker compose up` does not run them — apply once against the running container:
   ```
   dotnet ef database update --project src/TechQuiz.Infrastructure --startup-project src/TechQuiz.Api
   ```
   The dev `DataSeeder` (gated by `ASPNETCORE_ENVIRONMENT=Development` in the compose file) populates the Unit Testing category, 19 questions, and the demo user on startup. If you see startup logs like `Seeded category Unit Testing with 19 questions, quiz <guid>` and `Seeded demo user demo@techquiz.local`, you are good.

## Smoke

1. Import both JSON files into Postman.
2. Select the `TechQuiz Local` environment.
3. Run `Auth/Login (demo user)` — pre-filled with `demo@techquiz.local` / `Demo123!`. Expected: `200 OK` with `accessToken`, `accessTokenExpiresAt`, `refreshToken`, `refreshTokenExpiresAt`. The post-response script saves both tokens to the environment.
4. Run `Auth/Refresh` — picks up the saved `refreshToken`, rotates it (server-side: marks the old token revoked, issues a new pair), returns `200 OK`. Re-running `Refresh` against the *previous* token now fails (rotation = single-use).
5. (Optional) Run `Auth/Register` with the pre-filled payload to create a fresh user; receives a `200 OK` with tokens.

## Status & limits

- **Failure responses are temporarily ugly** — wrong password / duplicate email currently surface as `500 Internal Server Error` in Development mode (ASP.NET's developer exception page is enabled). Session 1.4-C introduces a `ProblemDetails`-shaped exception middleware that maps:
  - `RegistrationFailedException` → `400 Bad Request` with field errors
  - `UnauthorizedAccessException` → `401 Unauthorized`
  - `ValidationException` (FluentValidation) → `400 Bad Request` with per-field messages
- **No quiz endpoints yet** — categories, quiz lifecycle, attempt history land in session 1.4-B.
- **No Newman / CI runner** — session 1.4-D adds the Newman command + smoke script for CI.
