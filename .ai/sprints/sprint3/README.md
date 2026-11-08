# Sprint 3 — Phase 3: AI Integration

> Status: done — delivered as iterations 3.1–3.7 (see the Delivered section and `LOG.md`). The planned list below is the original pre-phase outline, kept as history.

## Phase goal
Enable AI-generated questions via user-supplied API keys, support multiple providers (OpenAI, Anthropic, Google), introduce code-style questions with Monaco Editor.

## Planned iterations

- **3.1 — AI provider abstraction**: Define `IAiProvider` interface. Implementations for OpenAI, Anthropic, Google. Strategy pattern for selecting provider based on user preference. Application-layer use cases call abstraction only.
- **3.2 — Encrypted key storage**: User-supplied API keys stored encrypted in DB via `IDataProtectionProvider`. Per-user, never logged, decrypted only at use time. Endpoint to add/remove/rotate keys.
- **3.3 — Settings screen UI**: User-facing settings page for managing AI keys, selecting active provider, viewing usage stats. Per ADR design tokens.
- **3.4 — Generate quiz screen UI**: Form for topic + difficulty + question count + provider. Triggers async generation, shows progress, displays generated questions for user review before persisting.
- **3.5 — Public pool**: AI-generated questions added to a public pool shared across users. Voting/flagging mechanism for quality control. Owner of question retains attribution.
- **3.6 — Code challenge UI**: Frontend for the execution-backed `CodeChallenge` introduced by ADR-018 — Monaco editor, run + grade against hidden tests with a compile gate (compile first, then evaluate success). See `3.6-code-challenge-ui.md`. The non-executing variants (CodeOutput, CodeFix, FillIn) and AI rubric feedback are deferred to later work (AI feedback → 3.7).
- **3.7 — AI feedback on code submissions**: On-demand qualitative AI feedback on a code-challenge submission, on the **editor screen** next to the test verdict (code challenges don't use the quiz Result screen — ADR-018). Complementary to the deterministic grader, never the pass/fail authority; uses the caller's own provider key (BYOK). See `3.7-ai-code-feedback.md`. Feedback for the non-executing variants and option-based quiz questions stays deferred.

## Delivered

- **3.1** delivered the `IAiProvider` port + resolver with a stub provider (#169). **3.2** delivered encrypted BYO-key storage + the **native Anthropic provider** (#178). ADR-019 set the target provider set as native OpenAI, Anthropic, Gemini plus OpenRouter; only Anthropic was built. OpenAI, Gemini and OpenRouter exist only as `AiProviderKind` values, and the resolver throws `UnknownAiProviderException` for them.
- **3.3** Settings UI (#182) — usage stats deferred (no usage endpoint). **3.4** Generate screen (#186). **3.5** Public pool — persist, publish (Draft → Published, ADR-020), browse (#196); voting/flagging/moderation and playing pooled questions were **not built**. **3.6** Code challenge UI (#203, with compile-gate #198) — graded by Judge0 execution against hidden tests (ADR-018). **3.7** AI code feedback (#210). Rate limiting (ADR-006 consequence) was deferred in 3.2 and not built.
- Phase 3 shipped before Phase 2 (2026-06-25 → 06-29).

## Mockups available
No dedicated Phase 3 mockups were produced; the screens were built against the existing design tokens and later restyled by the Momentum redesign (4.10, ADR-024). The code question forward-look is in `docs/mockups/quiz-code-output-*.html`.

## References
- ADR-003 Multi-user (per-user data isolation)
- ADR-006 Multi-Provider AI with User-Supplied Keys
- ADR-007 Public AI-Generated Question Pool
- ADR-013 MVP-first scope strategy
- ADR-018 Online Code Execution for Coding Questions
- ADR-019 AI Provider Set — Native OpenAI/Anthropic/Gemini plus OpenRouter
- ADR-020 Pool Persistence via Draft → Published Lifecycle

## Notes
**This is the riskiest phase.** AI provider APIs change, rate limits vary, prompts need tuning. Plan for at least one full iteration spent debugging prompt quality + response parsing. The encrypted key storage in 3.2 is **non-negotiable security work** — don't skip it because "it's just a portfolio project".

Code questions are the **most differentiated** feature — a portfolio with `internal` vs `public` quizzes is generic, but a portfolio where your code is compiled and run against hidden tests (Judge0, ADR-018), with optional AI feedback on top, is **specific and memorable**.
