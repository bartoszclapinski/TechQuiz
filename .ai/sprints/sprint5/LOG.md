# Sprint 5 — Log

Chronologiczny dziennik pracy w sprincie 5 (Phase 5: Post-launch, ADR-027).
Najnowsze wpisy na górze.

---

## 2026-11-08 — Iteracja 5.1: smoke + domknięcie (task 9)

**Cel:** sprawdzić 5.1 na prawdziwym API bez udziału ownera (brak czasu na klikanie) i domknąć iterację.

- **Pełny zestaw lokalnie z Testcontainers:** 459/459 (Domain 129, Application 200, Infrastructure 91, API 39).
- **Smoke na stacku z brancha:** fałszywy klucz OpenAI → prawdziwe API OpenAI → 401 → u nas **422** („AI provider
  rejected the key") dla generowania i dla feedbacku do kodu. W logach API zero `sk-proj-`, `sk-ant-` i końcówki klucza.
- **Do zrobienia przez ownera (#397):** run na doładowanym kluczu OpenAI, ponowne sprawdzenie Anthropic na kontrakcie
  `{ "questions": [...] }`, potem flip `available: true`.
- **Uwaga środowiskowa:** stary lokalny wolumen `techquiz_postgres-data` nie przechodzi migracji taksonomii (FK
  `fk_categories_tracks_track_id`) — dane sprzed 4.8. Smoke poszedł na osobnym projekcie Compose (`-p techquizsmoke`)
  z czystym wolumenem; wolumen ownera nietknięty. Reset: `docker compose down -v`, gdy lokalne dane nie są potrzebne.
- **Kosmetyka:** `detail` w ProblemDetails używa nazwy z enuma („OpenAi rejected…"); UI pokazuje własny komunikat,
  więc nie poprawiane.

**Następny krok:** merge #388, potem fix trwałego key ringu Data Protection (klucze AI nie przetrwają restartu na Render).

---

## 2026-11-08 — Iteracja 5.1: implementacja (taski 2–8)

**Cel:** natywny dostawca OpenAI na seamie `IAiProvider`, z tym samym zachowaniem, testami i obsługą błędów co Anthropic.

- **Task 2 (#389)** — wspólny helper `AiPrompts` (prompty, parsowanie, fence-stripping) wydzielony z `AnthropicAiProvider`;
  `CapturingHandler` do osobnego pliku. Czysty refaktor — testy Anthropic bez zmian, zielone.
- **Task 3 (#390)** — jeden kontrakt odpowiedzi `{ "questions": [...] }` (strict `json_schema` wymaga obiektu w korzeniu);
  goła tablica dalej akceptowana jako tolerancja. TDD: 2 nowe testy czerwone przed zmianą.
- **Task 4 (#391)** — `AiProviderRejectedException` (Application): `InvalidKey` → **422**, `QuotaOrRateLimit` → **429**
  (wcześniej 500). Nigdy 401 — SPA traktuje 401 jako wygasłą sesję. Anthropic: 401/403, 429 i 400 „credit balance".
  Komunikat to stały tekst, bez klucza i body z upstreamu (test na wyciek). Testy API z podmienionym providerem przez
  `WithWebHostBuilder` + `ConfigureTestServices`.
- **Task 5 (#392)** — `AiProviderWiringTests` na prawdziwym `AddInfrastructure` sprawdza host każdego dostawcy; klient
  Anthropic nazwany. Test mutacyjny: druga nienazwana rejestracja z adresem OpenAI → test czerwony.
- **Task 6 (#393)** — `OpenAiOptions` (`gpt-6-luna`, `reasoning_effort = low`, `max_completion_tokens`) + `OpenAiAiProvider`:
  surowy `HttpClient`, Bearer per request, `store: false`, strict `json_schema`; 401/403 → `InvalidKey`, 429 (także
  `insufficient_quota`) → `QuotaOrRateLimit`; refusal / `finish_reason: "length"` / brak treści → `AiResponseException`.
  18 testów; przypadek OpenAI w teście DI czerwony do czasu rejestracji klienta `"OpenAi"`.
- **Task 7 (#394, #395)** — `providerLabel()` (Generate, feedback, Pool), komunikaty 422/429, `<select>` dostawcy dla
  feedbacku do kodu (wybór jako stan, dostawca wyliczany — bez `useEffect`). Flaga OpenAI dalej `false`.
- **Task 8** — docs: `ARCHITECTURE.md` (prawdziwe nazwy klas, kształt `IAiProvider`, nazwane klienty, kontrakt 422/429),
  `QUICK_START.md`, `ONBOARDING.md`, `sprint3/README.md`; DoD 5.1 odhaczone poza live smoke.

**Weryfikacja:** CI zielone na każdym commicie (Domain 129, Application 200, Infrastructure 91, API 39); lokalnie bez
Dockera tylko testy bez Testcontainers. `pnpm build` + `pnpm lint` czyste.

**Następny krok:** task 9 — stack z brancha, live smoke Anthropic na nowym kontrakcie + fałszywy klucz OpenAI (401 → 422),
przegląd logów pod kątem kluczy.

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
