# Sprint 5 — Log

Chronologiczny dziennik pracy w sprincie 5 (Phase 5: Post-launch, ADR-027).
Najnowsze wpisy na górze.

---

## 2026-11-08 — Iteracja 5.1: plan (task 1)

**Cel:** otworzyć Phase 5 i zaplanować natywnego dostawcę OpenAI na istniejącym seamie `IAiProvider`.

- Phase 5 „Post-launch" uzgodniona z ownerem: 5.1 OpenAI, 5.2 Gemini, 5.3 OpenRouter, 5.4 auth hardening
  (#66, #67, #68, #99). Zapisana w **ADR-027** (amenduje ADR-013; ADR-013 dostaje „Amended by ADR-027").
- Plik iteracji `5.1-openai-provider.md` + outline `README.md`. Plan powstał z audytu seamu AI (backend,
  frontend, konwencje, aktualne API OpenAI) i dwóch recenzji (techniczna + procesowa).
- **Decyzje ownera:** nowa faza zamiast 3.8; `<select>` dostawcy dla feedbacku do kodu (dziś zawsze wygrywa
  Anthropic); brak klucza OpenAI z doładowaniem → 5.1 merge'uje się z OpenAI dalej jako „soon", a run na
  prawdziwym kluczu + flip flagi idą osobnym małym PR-em.
- **Decyzje techniczne:** `v1/chat/completions` (wspierane, ten sam kształt co OpenRouter w 5.3), strict
  `json_schema` → wspólny kontrakt `{ "questions": [...] }` dla obu dostawców, model `gpt-6-luna` z
  `reasoning_effort = low` w `OpenAiOptions`, typowane błędy 422/429 także dla Anthropic (dziś 500).
- **Znalezisko:** dwa nienazwane `AddHttpClient<IAiProvider, …>` nie rzucają wyjątku — dzielą nazwę i wygrywa
  ostatni `BaseAddress`, więc Anthropic wysyłałby zapytania do OpenAI. Stąd nazwane klienty + test DI
  sprawdzający host każdego dostawcy (task 5).
- Roadmapa zaktualizowana w `CLAUDE.md`, `.ai/README.md`, `QUICK_START.md`, `ONBOARDING.md`.

**Następny krok:** task 2 — wydzielenie wspólnego helpera promptów z `AnthropicAiProvider` (czysty refaktor,
testy Anthropic bez zmian).
