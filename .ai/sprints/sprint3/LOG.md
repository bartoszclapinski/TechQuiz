# Sprint 3 — Session Log

> Chronological record of work done in Sprint 3 (Phase 3 — AI integration + code questions).
> Each entry: date, what was done, decisions made, verification result.
> Iteration plans live in `.ai/sprints/sprint3/X.Y-*.md`.

---

## 2026-06-25 — Iteration 3.1 setup: plan expansion + branch

**Co zrobione:**
- Branched `feat/ai-provider-abstraction` off master. One PR will close iteration 3.1.
- Expanded `3.1-ai-provider-abstraction.md` from outline to full goal + DoD + ordered task list.
- Started this Sprint 3 `LOG.md`.

**Decyzje:**
- **Scope 3.1 = the seam, not the vendors.** Iteration ships `IAiProvider` + provider resolver + `GenerateQuestionsCommand`, all TDD against a fake provider — no network, no key. Real provider HTTP clients are Infrastructure work (ADR-008 → integration-tested), pushed to 3.2.
- **Native client: Anthropic only.** It is the only provider the owner can fund/test today. Building OpenAI/Google clients that can't be run would be untested code — skipped deliberately.
- **OpenRouter as the multi-model path.** One OpenAI-compatible endpoint (GPT / Gemini / open models on a single key) lands later as a second `IAiProvider`, better ROI than three bespoke clients. It changes ADR-006's per-provider-key model, so it requires a **new ADR amending ADR-006** before wiring (hard rule #5).

**Weryfikacja:**
- Plan + LOG committed on the feature branch; no code yet (TDD cycle begins next).

---

## 2026-06-25 — Iteration 3.1 implementation: provider seam + use case

**Co zrobione (3 atomic commits, TDD):**
- **Provider seam** (`#165`) — `IAiProvider` port + `GenerateQuestionsRequest`/`GeneratedQuestionDraft` DTOs, `AiProviderKind` enum (Anthropic, OpenRouter), `IAiProviderResolver` + `AiProviderResolver` (indexes providers by reported `Kind`, throws `UnknownAiProviderException`, rejects duplicate kinds). 3 resolver tests.
- **Use case** (`#166`) — `GenerateQuestionsCommand` + `GenerateQuestionsResult`, handler (resolve → map → call provider once → tag result; no persistence; errors propagate), `GenerateQuestionsCommandValidator` (topic non-empty, count 1–20, difficulty/provider in-enum). 13 tests.
- **Infra wiring** (`#167`) — `StubAiProvider` (deterministic, no-network, Kind=Anthropic placeholder for 3.2) + DI registration in `AddInfrastructure`; light DI smoke test via a real `ServiceProvider`. 2 tests.

**Decyzje:**
- **`AiProviderResolver` lives in Application, not Infrastructure.** It is pure selection logic over the injected `IEnumerable<IAiProvider>` — provable without a container. Only the concrete providers (and the stub) live in Infrastructure per ADR-008.
- **Stub registered as `Kind=Anthropic`** so the seam is usable end-to-end now; 3.2 swaps it for the real Anthropic client without touching the resolver or use case.
- Fixed a plan-doc typo: `AiProviderKind` is (Anthropic, OpenRouter), not (OpenAi, Anthropic).

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → 0 warnings / 0 errors.
- `dotnet test TechQuiz.Application.Tests` → 99/99 green (18 new for AI: 16 Application + 2 Infrastructure DI smoke). Full Infrastructure suite (Testcontainers) deferred to CI — these AI tests need no Docker.

---

## 2026-06-25 — Iteration 3.2 start: BYO-key Application slice

**Co zrobione:**
- Closed iteration 3.1, added **ADR-019** (four-provider set: native OpenAI/Anthropic/Gemini + OpenRouter, amends ADR-006) and wrote the full `3.2-byok-and-anthropic.md` plan. Branched `feat/ai-key-storage`.
- **Key management Application slice** (`#172`, 1 commit, TDD) — `IAiKeyStore` port; `SetAiKeyCommand` / `RemoveAiKeyCommand` / `GetConfiguredProvidersQuery` + handlers + validators, all scoped to the current user via `IUserContext`; extended `AiProviderKind` with `OpenAi`, `Gemini`; added `MissingAiKeyException`. 25 AI Application tests green.

**Decyzje:**
- **Four native/gateway providers, not a gateway-only shortcut.** BYO-key must honor the key a user already holds — forcing an OpenAI user onto OpenRouter (new account + separate funding) is bad UX. Enum carries all four now; only Anthropic is built/verified live in 3.2, the rest are deferred but the resolver's unknown-kind path stays real.
- **List/Get expose kinds only, never key material** (ADR-006). Encryption at rest lands with the EF persistence commit next.

**Weryfikacja:**
- `dotnet test TechQuiz.Application.Tests --filter Features.Ai` → 25/25 green.

---

## 2026-06-25 — Iteration 3.2: encrypted persistence + live Anthropic provider

**Co zrobione:**
- **Encrypted key store** (`#173`) — `UserAiKey` row (composite PK (user, provider), provider as text, cascade FK), `EncryptedAiKeyStore` over ASP.NET Data Protection, `AddUserAiKeys` migration, `AddDataProtection()` + scoped registration. 6 Testcontainers tests, incl. one asserting the stored column is ciphertext, not plaintext.
- **Anthropic provider + key-injection seam** — `IAiProvider.GenerateQuestionsAsync` now takes the per-user `apiKey` (provider stays a stateless singleton). `GenerateQuestionsCommandHandler` fetches the key from `IAiKeyStore` and throws `MissingAiKeyException` when none is configured. `AnthropicAiProvider` (typed HttpClient → Messages API, prompts for a strict JSON array, tolerates ``` fences, parses to drafts), `AnthropicOptions`, `AiResponseException`. DI swaps the stub for the real client; resolver moved to **scoped** so it never captures the typed HttpClient. 8 provider/registration tests (mocked `HttpMessageHandler`, no network) + handler missing-key test.

**Decyzje:**
- **Key flows as a method argument, not on the provider.** The handler (Application) owns the "user must have a key" policy via `MissingAiKeyException`; the provider (Infrastructure) is pure HTTP and attaches `x-api-key` per request. Keeps the provider a safe singleton and the policy testable without HTTP.
- **Resolver is now scoped.** A singleton resolver capturing a typed-HttpClient provider would freeze handler rotation — scoping the resolver avoids the captive dependency.
- **Default key ring is host-local (noted in DI).** Staging/prod must persist the Data Protection ring (Azure Blob + Key Vault) or rotated keys become undecryptable on restart — deploy-time follow-up, out of 3.2 scope.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → 0/0. Application Ai filter → 26/26; Infrastructure provider+registration → 8/8; encrypted-store Testcontainers → 6/6.

---

## 2026-06-25 — Iteration 3.2: key-management API endpoints

**Co zrobione:**
- `AiKeysController` at `api/ai/keys` — `PUT` (set/rotate), `DELETE /{provider}` (remove), `GET` (list configured). All `[Authorize]`. `MissingAiKeyException` mapped to 409 in `GlobalExceptionHandler` so generation without a key can never 500.
- 4 E2E tests through `WebApplicationFactory` + Postgres: auth required, set→list→remove round-trip, unknown provider → 400, empty key → 400.

**Decyzje:**
- **Providers cross the wire as enum names ("Anthropic"), not ints.** The quiz/results endpoints rely on the API having *no* global string-enum converter (the React client hard-codes `Difficulty = {Easy:0,…}`), so flipping global serialization would break the frontend. The AI controller maps provider↔string itself to stay readable without that global change.
- **Unknown provider handled at the controller (400), empty key by the existing validation pipeline (400).** Key material is only ever accepted, never returned by `GET`.

**Weryfikacja:**
- AiKeys API tests → 4/4 green (real encryption + Postgres round-trip).
