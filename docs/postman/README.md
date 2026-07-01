# TechQuiz API — Postman collection

Auth endpoints delivered in iteration 1.4 session A; quiz endpoints in session 1.4-B.

## Files

- `TechQuiz.postman_collection.json` — two folders:
  - `Auth/` — three POSTs: register, login, refresh.
  - `Quiz/` — the full play-through: get categories, start, submit answer, complete, get result, get attempt history.
- `TechQuiz.local.postman_environment.json` — `baseUrl`, `accessToken`, `refreshToken` for the local docker-compose stack.

Login + refresh include post-response scripts that stash `accessToken` and `refreshToken` into the active Postman environment. The Quiz requests send `Authorization: Bearer {{accessToken}}` and chain via post-response scripts: `Get Categories` stashes `categoryId`, `Start Quiz` stashes `attemptId` plus the first `questionId`/`selectedOptionId`, so the remaining requests run without copy-paste.

## Prerequisites

1. Containers up:
   ```
   docker compose up -d
   ```
   Brings up Postgres on `127.0.0.1:5433` and the API on `127.0.0.1:8085`.

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
6. With a valid `accessToken` in the environment, run the `Quiz/` folder top to bottom:
   - `Get Categories` → `200 OK` list; stashes the first `categoryId`.
   - `Start Quiz` → `200 OK` with `attemptId` + questions (no `isCorrect` on options — Hard Rule #4); stashes `attemptId`, first `questionId`, first `selectedOptionId`.
   - `Submit Answer` → `204 No Content`. Re-run with a different question/option to answer more.
   - `Complete Quiz` → `200 OK` `QuizResultDto` (score, per-question breakdown — `isCorrect` is exposed here, on the result view only).
   - `Get Result` → `200 OK`, same result, read-only (re-fetchable).
   - `Get Attempt History` → `200 OK` paginated list including the attempt you just completed.

## Status & limits

- **Failure responses are RFC 7807 ProblemDetails** (session 1.4-C). Errors carry `type`/`title`/`status`/`detail`/`traceId`, and validation failures add a per-field `errors` map. Status mapping:
  - `ValidationException` (FluentValidation) / `RegistrationFailedException` / `ArgumentException` → `400 Bad Request`
  - `UnauthorizedAccessException` (bad login, invalid/expired refresh) → `401 Unauthorized`
  - `ForbiddenAccessException` (acting on someone else's attempt) → `403 Forbidden`
  - `KeyNotFoundException` (missing attempt/quiz) → `404 Not Found`
  - `DomainException` (e.g. result requested before the quiz is completed) → `409 Conflict`
  - anything else → `500` with a generic message (no stack trace leaked)
- **Interactive API reference (Scalar)** at `http://127.0.0.1:8085/scalar/v1` in Development — use the **Authorize** button to paste the access token from login, then call secured endpoints from the browser.
- **CORS** is enabled for the Vite dev origin `http://localhost:5173` (with credentials, for the refresh cookie).
- **Newman runner** (session 1.4-D). With the stack up (`docker compose up -d`), run the whole
  collection headless from this folder:
  ```
  npm install   # first time only
  npm run smoke
  ```
  This executes `Auth/` then `Quiz/` against `{{baseUrl}}` (`http://localhost:8085`) and chains the
  saved tokens, so it is the same play-through as the manual Postman steps above. The
  `.github/workflows/api-smoke.yml` workflow runs the identical command on a manual dispatch
  against a CI-booted API; it is not a per-PR gate because the full flow is already covered in
  process by the `TechQuiz.Api.Tests` `WebApplicationFactory` integration tests.
