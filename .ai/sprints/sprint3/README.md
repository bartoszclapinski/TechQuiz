# Sprint 3 — Phase 3: AI Integration

> Status: outlined (detail expansion deferred until current phase completes)

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

## Mockups available
None for Phase 3 yet. Mockups deferred to early in Phase 3 work, after architecture is clearer. Code question forward-look exists in `mockups/quiz-code-output-*.html` from Phase 1 UI design session.

## References
- ADR-003 Multi-user (per-user data isolation)
- ADR-013 MVP-first scope strategy

## Notes
**This is the riskiest phase.** AI provider APIs change, rate limits vary, prompts need tuning. Plan for at least one full iteration spent debugging prompt quality + response parsing. The encrypted key storage in 3.2 is **non-negotiable security work** — don't skip it because "it's just a portfolio project".

Code questions are the **most differentiated** feature — a portfolio with `internal` vs `public` quizzes is generic, but a portfolio where AI evaluates whether your code handles edge cases is **specific and memorable**.
