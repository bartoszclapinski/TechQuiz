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

---

## 2026-06-25 — Iteration 3.2: generate endpoint (closes the vertical)

**Co zrobione:**
- `AiQuestionsController` at `POST api/ai/questions` — `[Authorize]`, accepts `{topic, difficulty, count, provider}` with difficulty/provider as enum *names*, maps to `GenerateQuestionsCommand`, returns `{provider, questions[]}`. This is the HTTP surface the live smoke (DoD line 8) drives.
- 4 E2E tests through `WebApplicationFactory` + Postgres: unauthenticated → 401; no configured key → 409 (deletes the Anthropic key first to guarantee the no-key state, so the test needs **no** real LLM call); unknown provider → 400; unknown difficulty → 400.

**Decyzje:**
- **Response omits `CorrectOptionIndex`.** Hard rule #4 + the `GenerateQuestionsResult` contract keep correct-answer indices server-side; the smoke only needs to prove Claude authored stems/options. Draft DTO carries stem, options, difficulty name, explanation — not the answer key.
- **Difficulty + provider parsed as strings in the controller**, same rationale as `AiKeysController`: no global string-enum converter (the React quiz client hard-codes numeric `Difficulty`), so each AI action maps its own enums. Unknown values → 400 before reaching MediatR.
- **Live smoke stays the owner's manual step.** It needs a funded Anthropic key, so it can't be automated; DoD line 8 documents the exact `PUT /api/ai/keys` → `POST /api/ai/questions` sequence. All other DoD boxes are checked.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → 0/0. AiQuestions API tests → 4/4. Full solution → 226/226 green (Domain 65, Application 109, Infrastructure 35, Api 17).

---

## 2026-06-25 — Iteration 3.2: live smoke PASSED (DoD closed, status → done)

**Co zrobione:**
- Ran the end-to-end BYO-key vertical against the **real Anthropic API** with the owner's key: login (demo user) → `PUT /api/ai/keys` (Anthropic) → `POST /api/ai/questions {topic:"C# records", difficulty:"Easy", count:3}`. Claude returned **3 well-formed drafts** (4 options each, correct difficulty). `GET /api/ai/keys` → `["Anthropic"]` only; **zero `sk-ant` occurrences in the API log** (key never logged). Iteration 3.2 status flipped to **done**.

**Dwa gotchas środowiska dev (nie kod, ale warto zapamiętać):**
- **Port:** compose publikuje Postgresa na **5433** (`127.0.0.1:5433->5432`, by nie kolidować z lokalnym 5432), a `appsettings.json` ma `Port=5432`. To działa container-to-container w compose; przy lokalnym `dotnet run` trzeba nadpisać connection string na 5433 (env `ConnectionStrings__DefaultConnection`). Design-time factory już domyślnie celuje w 5433.
- **Migracje:** API **nie robi auto-migracji na starcie**. Trwała dev-baza (wolumen `postgres-data`) była migrowana przed dodaniem `AddUserAiKeys`, więc `PUT /api/ai/keys` dawało 500 `relation "user_ai_key" does not exist` do czasu ręcznego `dotnet ef database update`. Po aplikacji migracji smoke przeszedł.

**Uwaga bezpieczeństwa:** klucz właściciela siedzi teraz zaszyfrowany w dev-bazie (per-user, BYO-key). Można go usunąć przez `DELETE /api/ai/keys/Anthropic`; przeżywa restarty dopóki nie zrobi się `docker compose down -v`.

---

## 2026-06-25 — Iteration 3.3 start: plan + branch

**Co zrobione:**
- Closed 3.2 (merged PR #178 to master at `6b9ba65`). Branched `feat/settings-ai-keys` off clean master.
- Expanded the Sprint 3 outline into the full `3.3-settings-ui.md` plan (goal + DoD + ordered task list).

**Decyzje (zakres uzgodniony z właścicielem):**
- **3.3 = UI only nad istniejącym kontraktem `/api/ai/keys`.** Cała ochrona klucza (szyfrowanie at-rest, per-user, never-logged/never-returned) już jest w 3.2 — frontend jest cienkim klientem. Klucz nigdy nie ląduje w cache/localStorage ani nie jest pokazywany po wpisaniu; UI renderuje tylko stan „Configured / Not configured" z `GET`.
- **Tylko Anthropic konfigurowalny teraz.** OpenAI/Gemini/OpenRouter renderowane jako „soon" (disabled) — spójnie z patternem `COMING_SOON` w topbarze (ADR-014). Brak sensu pozwalać zapisać klucz pod providera, którego nie ma jak użyć.
- **Świadomie odłożone:** wybór aktywnego providera → 3.4 (ekran Generate); statystyki użycia → brak backendu (żaden endpoint usage-tracking nie istnieje).

**Weryfikacja:**
- Plan committed on the feature branch; implementacja (feature folder + strona + routing) idzie następnym commitem. Closes #179.

---

## 2026-06-25 — Iteration 3.3: settings page built + verified (DoD closed, status → done)

**Co zrobione:**
- `web/src/features/settings/` — `api.ts` (`fetchConfiguredProviders`/`setAiKey`/`removeAiKey` + `AI_PROVIDERS` catalog), `query-keys.ts` (`aiKeysKey`), trzy hooki (`useConfiguredProviders` query, `useSetAiKey`/`useRemoveAiKey` mutacje z invalidacją), `settings-page.tsx`. Routing `/settings` w `App.tsx` (za `RequireAuth`+`AppShell`), gear `NavLink` w topbarze. Closes #180.
- Status 3.3 → done, wszystkie DoD odhaczone. Closes #181.

**Decyzje:**
- **Gear w prawym klastrze, nie w głównej nawigacji.** Settings to utility, nie destynacja „feature" — siedzi obok theme toggle, a `COMING_SOON` (Generate/Dashboard/…) zostaje nietknięte.
- **Klucz znika ze stanu komponentu w `onSuccess`** — API i tak go nie zwraca; input czyści się od razu, nic nie ląduje w cache/localStorage. UI renderuje tylko `["Anthropic"]` z `GET`.
- **Empty key łapany client-side (inline), API 400 też inline** (`messageFromSetError`), nie tylko toast — zgodnie z DoD.
- **Remove = inline two-step confirm** zamiast osobnego dialogu (mniej ceremoniału, soft pref #5).

**Weryfikacja:**
- `pnpm build` (`tsc -b` strict + `vite build`) → czysto, 0 błędów.
- Kontrakt API zsmoke'owany bezpośrednio: login demo → `GET /api/ai/keys` → `["Anthropic"]` (200).
- **Owner kliknął round-trip w przeglądarce (dev: vite 5173 → API 5032):** Settings pokazuje Anthropic „Configured" + trzy „soon"; pusty klucz → inline „Enter a key."; rotate → toast + czyszczenie inputu; remove z potwierdzeniem → „Not configured"; dark/light OK. Potwierdzone: „wszystko zadziałało".

**Gotcha dev (nie kod):** profil `http` z `launchSettings.json` (applicationUrl=5032) nadpisuje `ASPNETCORE_URLS`, więc `dotnet run --launch-profile http` ląduje na **5032**, nie 8080. Frontend domyślnie celuje w 8080 — przy lokalnym `dotnet run` trzeba albo `--urls http://localhost:8080`, albo wskazać front na 5032 przez `VITE_API_BASE_URL`. Port API nie wpływa na CORS (origin = 5173, już dopuszczony).

---

## 2026-06-25 — Iteration 3.4 start: plan + branch

**Co zrobione:**
- Closed 3.3 (merged PR #182 to master). Branched `feat/generate-quiz-ui` off master.
- Expanded the Sprint 3 outline into the full `3.4-generate-quiz-ui.md` plan (goal + DoD + ordered tasks).

**Decyzje (zakres uzgodniony z właścicielem — „zgodnie z rekomendacją, poprawimy później"):**
- **3.4 = generate + preview (read-only).** `POST /api/ai/questions` już zwraca drafty bez zapisu; persystencja (public pool, atrybucja, voting) to 3.5. „Save to my pool" w podglądzie = disabled „soon", żeby działający efekt klucza był widać teraz, a zapis doszedł osobno.
- **Difficulty + provider jako *nazwy*** ("Easy", "Anthropic") — endpoint AI nie ma globalnego string-enum convertera (quiz zależy od numerycznego Difficulty). Ten feature pracuje na nazwach, świadomie oddzielony od numerycznego quizu.
- **Bez klucza odpowiedzi w podglądzie** — draft DTO celowo pomija correct option (hard rule #4). Pokazanie odpowiedzi autorowi to decyzja kontraktowa do rozważenia przy persystencji (3.5), nie cichy workaround front-endowy.
- **„Generate" awansuje do głównej nawigacji** (destynacja feature), w odróżnieniu od Settings (utility, gear). No-key path kieruje do Settings zamiast pewnego 409.

**Weryfikacja:**
- Plan committed on the feature branch; implementacja (feature folder + strona + routing) idzie następnym commitem. Closes #183.

---

## 2026-06-25 — Iteration 3.4: generate screen built + verified (DoD closed, status → done)

**Co zrobione:**
- `web/src/features/generate/` — `api.ts` (`generateQuestions` + `GenerateRequest`/`GeneratedDraft`/`GenerateResult`, difficulty/provider jako nazwy, draft bez klucza odpowiedzi), `use-generate-questions.ts` (mutacja, brak cache — drafty efemeryczne do 3.5), `generate-page.tsx` (formularz RHF+zod, bramka providera, podgląd draftów). Routing `/generate` w `App.tsx`. „Generate" awansowane z `COMING_SOON` do głównej nawigacji. Closes #184.
- Status 3.4 → done, DoD odhaczone. Closes #185.

**Decyzje:**
- **Brak `query-keys.ts` w generate** — generacja to mutacja bez własnego query; provider list reużywa `useConfiguredProviders`/`aiKeysKey` z 3.3. Pusty plik kluczy byłby martwy (pragmatyzm > sztywne trzymanie się listy plików z planu).
- **`z.number({ error })` + `register('count', { valueAsNumber: true })`** zamiast `z.coerce.number()` — coerce rozjeżdża input/output typy schematu i wywala zodResolver. (Zod v4: `invalid_type_error` → `error`.)
- **No-key path = osobny widok** („No provider configured" + link do Settings), nie submit w pewny 409. 409/400/network → toasty (`reportGenerateError`).

**Weryfikacja:**
- `pnpm build` (`tsc -b` strict + `vite build`) → czysto.
- **Owner kliknął w przeglądarce (dev: vite 5173 → API 5032):** formularz + walidacja + **generacja na żywo przeciw realnemu Anthropic** zwróciła drafty end-to-end; „Save to my pool" disabled „soon"; dark/light OK. Potwierdzone: „wszystko działa poprawnie".
- **No-key path zweryfikowany nieinwazyjnie:** świeży keyless user → `GET /api/ai/keys` → `[]` (200), czyli dokładnie te dane, na których UI renderuje notice + link do Settings. Klucza właściciela nie ruszano (ma tylko jeden i nie trzyma jego wartości pod ręką — usunięcie = utrata).

---

## 2026-06-25 — Iteration 3.5 start: scope, ADR-020, plan + branch

**Co zrobione:**
- Closed 3.4 (merged PR #186 to master). Branched `feat/public-pool-persistence` off master.
- Expanded the Sprint 3 outline into the full `3.5-public-pool.md` plan (goal + DoD + ordered tasks).
- Added **ADR-020** (refines ADR-007) recording the persistence mechanism + the contract change.

**Decyzje (zakres uzgodniony z właścicielem — „zgodnie z rekomendacją"):**
- **3.5 = persist + attribution + browse.** Voting, flagging, moderation queue oraz *grywalność*
  pytań z puli (mapowanie topic→category, wpięcie w runner) — odłożone (ADR-007, Phase 3/4).
- **Mechanizm zapisu = Draft → Published (Opcja B, ADR-020).** Generacja od razu persystuje drafty
  jako `Draft` (właściciel = autor, serwer trzyma `CorrectOptionIndex`), „Save to my pool" = publish
  → `Published` (widoczne dla wszystkich). Daje kuracyjną bramkę i naturalne miejsce na moderację.
- **Dlaczego nie Opcja A (generacja = publish od razu):** prostsza, ale każdy śmieć ląduje w
  wspólnej puli bez bramki, a moderacji jeszcze nie ma. Status to jedno pole — tani zysk.
- **Zmiana kontraktu `POST /api/ai/questions`** (teraz *zapisuje*) zapisana w ADR-020, nie po cichu
  (hard rule #5). Odpowiedź do klienta bez zmian co do klucza odpowiedzi (hard rule #4 trzyma w obu
  stanach) — dochodzi tylko `id` draftu, żeby front mógł publikować po id, nie po treści.

**Następny krok:**
- Domain TDD: agregat `PooledQuestion` (factory + `Publish()`), red→green→refactor.

---

## 2026-06-25 — Iteration 3.5: full vertical built (Domain → frontend), 6 atomic commits

**Co zrobione (6 atomic commits, 1 issue ↔ 1 commit):**
- **Domain** (`#188`, TDD) — `PooledQuestion` aggregate + `PooledQuestionOption` + `PooledQuestionStatus`
  (Draft/Published). Factory mirrors `Question` validation (≥2 options, exactly one correct for
  MultipleChoice) plus attribution (user, provider name, `GeneratedAtUtc`); starts `Draft`; `Publish()`
  Draft→Published, double-publish throws `PooledQuestionAlreadyPublishedException`. 16 tests.
- **Application** (`#189`, TDD) — `GenerateQuestionsCommandHandler` now *persists* drafts as `Draft`
  via `IPooledQuestionRepository` (+ `TimeProvider`, `IUserContext`), maps each draft → aggregate
  (correct option from `CorrectOptionIndex`, provider = `AiProviderKind.ToString()`); returns
  answer-key-free `GeneratedQuestionSummary` (now with draft `id`). `PublishPooledQuestionCommand`
  (+validator): load by id → `KeyNotFoundException`; not owner → `ForbiddenAccessException`; `Publish()`;
  save. `ListPooledQuestionsQuery` → `PooledQuestionDto` without correct option.
- **Infrastructure** (`#190`) — EF config (string-converted enums, indexed Status + CreatedByUserId,
  shadow-FK owned options via backing field), `PooledQuestionRepository`, DI registration,
  `AddPooledQuestions` migration. 3 Testcontainers round-trips incl. Draft→Published transition.
- **API** (`#191`) — `GET /api/pool/questions` + `POST /api/pool/questions/{id}/publish` (thin MediatR;
  401/403/404/409 all map through the existing `GlobalExceptionHandler`); generate response carries
  draft ids. 4 WebApplicationFactory smoke tests.
- **API refactor** (`#192`) — pool browse projects Difficulty to its enum *name* so the wire matches
  the generate preview (no global string-enum converter — the quiz client needs numeric Difficulty).
- **Frontend** (`#193`) — `web/src/features/pool/` (api + `usePooledQuestions` query +
  `usePublishQuestion` mutation + browse page), `/pool` route + nav entry. Generate preview now has a
  per-draft **Publish to pool** button (mutation → toast → pool-query invalidation; flips to
  "Published ✓", can't re-fire and 409).

**Decyzje:**
- **Provider trzymany jako *nazwa* (string) na agregacie, nie `AiProviderKind`** — `AiProviderKind`
  żyje w Application.Abstractions, więc trzymanie enuma w Domain złamałoby hard rule #3. Domain
  zapisuje proweniencję jako string; handler mapuje `.ToString()`.
- **Publish per-draft, nie „publish all"** — każdy draft to osobno persystowane pytanie z własnym id;
  przycisk per-karta wprost odzwierciedla model (i 409 przy ponownej publikacji jest naturalny).
- **Difficulty jako nazwa też w puli** (`#192`) — spójność z preview generacji; front reużywa tę samą
  mapę odznak (`Easy/Medium/Hard`).
- **Answer-key boundary:** correct option trzymany serwerowo na agregacie od generacji; publish działa
  po **id**, klient nigdy nie odsyła treści — hard rule #4 trzyma w obu stanach.

**Weryfikacja:**
- Domain 81/81, Application 118/118 green; `PooledQuestionRepositoryTests` 3/3 (Testcontainers);
  `PoolEndpointsTests` 4/4 (WebApplicationFactory + Postgres). `dotnet build` 0/0.
- Frontend: `pnpm lint` czysto, `tsc --noEmit` czysto, `pnpm build` (`tsc -b` + `vite build`) czysto.
- **Owner przeklikał golden path w przeglądarce (dev: vite 5173 → API 8080):** login demo →
  Generate (klucz Anthropic „Configured") → **Publish to pool** (toast + „Published ✓") → zakładka
  **Pool** pokazuje opublikowane pytanie bez klucza odpowiedzi; dark/light OK. Potwierdzone:
  „działa wszystko jak należy". DoD zamknięte, status 3.5 → **done**.

**Gotcha dev (nie kod):** trwała dev-baza (wolumen `postgres-data`) nie miała migracji
`AddPooledQuestions` — API nie auto-migruje, więc trzeba było ręcznie `dotnet ef database update`
przed klikaniem (inaczej `GET /api/pool/questions` → 500 `relation "pooled_questions" does not exist`,
ten sam wzorzec co przy `AddUserAiKeys` w 3.2). Drugi drobiazg: zombie-vite z poprzedniej sesji
trzymał 5173, więc świeży vite wpadał na 5174 (poza CORS allowlist API = 5173) — ubity, restart na 5173.
