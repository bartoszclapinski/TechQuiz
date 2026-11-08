# Sprint 5 — Phase 5: Post-launch

> Status: in progress (agreed with the owner 2026-11-08, see Notes). Only 5.1 has a detailed iteration file so far.

## Phase goal
Finish the ADR-019 provider set on the existing `IAiProvider` seam: native OpenAI, then native Gemini, then OpenRouter. Each provider is verified against its real API before it is claimed to work. Then close the auth-hardening follow-ups that have been open since Phase 1 (#66, #67, #68, #99).

## Planned iterations

- **5.1 — OpenAI provider**: Native `OpenAiAiProvider` on a raw typed `HttpClient`, sharing prompts and parsing with Anthropic. Both providers get typed invalid-key and quota errors. OpenAI is un-gated in Settings only after a funded-key run (follow-up #397). See `5.1-openai-provider.md`.
- **5.2 — Gemini provider**: Native Google Gemini client on the same seam, reusing the shared prompts and error mapping from 5.1. Gemini is un-gated in Settings.
- **5.3 — OpenRouter provider**: OpenAI-compatible client next to the native providers, not replacing them (ADR-019). OpenRouter honours strict `json_schema` only on some models, so 5.3 may need `provider.require_parameters` or a prompt-only fallback. OpenRouter is un-gated in Settings.
- **5.4 — Auth hardening**: refresh-token reuse detection with family revocation (#66), optimistic concurrency on rotation (#67), a cleanup job for expired/revoked tokens (#68), no refresh token in the JSON body for the browser flow (#99).

## Mockups available
None needed. The provider rows in Settings (3.3, restyled in 4.10) and the Generate provider picker (3.4) already exist. Each provider iteration flips one provider's `available` flag.

## References
- ADR-006 Multi-Provider AI with User-Supplied Keys
- ADR-008 TDD for Domain and Application Layers
- ADR-013 Scope Strategy — MVP First, Iterate (defines Phases 0–4 only)
- ADR-019 AI Provider Set — Native OpenAI/Anthropic/Gemini plus OpenRouter
- ADR-022 Staging Host — Render + Neon
- `.ai/sprints/sprint3/3.2-byok-and-anthropic.md` and `3.7-ai-code-feedback.md` (provider and feedback precedent)

## Notes
**Phase 5 is off the ADR-013 roadmap.** ADR-013 defines four phases, and all four are delivered. Phase 5 was agreed with the owner on 2026-11-08 and is recorded in ADR-027, which amends ADR-013.
- Recommendation: add ADR-027 *amending* ADR-013 (Phase 5 added; Phases 0–4 unchanged), and add `**Amended by ADR-027**` to ADR-013's status line, following the ADR-006 → ADR-019 precedent (hard rule #5).
- The plan commit (5.1 task 1) also updates the roadmap text in CLAUDE.md, `.ai/README.md`, `.ai/QUICK_START.md` and `.ai/ONBOARDING.md`.

**Labels.** Create `phase:5` and each `iteration:5.x` as that iteration starts, copying colour and description style from `phase:4` / `iteration:4.11`. When 5.4 starts, #66, #67, #68 and #99 keep `phase:1` (where they came from) and gain `iteration:5.4`; relabelling is the owner's call.

**LOG.** `sprint5/LOG.md` is created in 5.1 task 1: newest entry first, in Polish per CLAUDE.md's iteration workflow (step 6) and the sprint4 log.

**Live verification.** Per ADR-019, a provider stays "mocked-HTTP verified, live pending" until the owner runs it with a real key. Gemini and OpenRouter wait for their keys the same way.
