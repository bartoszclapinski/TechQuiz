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
