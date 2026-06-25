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
