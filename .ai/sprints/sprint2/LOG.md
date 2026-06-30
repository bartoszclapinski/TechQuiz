# Sprint 2 — Log

Chronologiczny dziennik pracy w sprincie 2 (Phase 2: Dashboard + Spaced Repetition).
Najnowsze wpisy na górze.

---

## 2026-06-30 — Iteracja 2.3: Time-range filter (Week / Month / All)

**Cel:** segmented control Week / Month / All time na Dashboardzie. Zakres filtruje agregaty;
backend dokłada param do istniejącego `GET /api/dashboard` (jeden endpoint), frontend cache'uje
każdy zakres osobno.

**Co zrobione (plan + 3 atomic commits):**
- **Plan** (`#222`) — pełny plik iteracji 2.3 (semantyka okien, empty-state, DoD).
- **Backend** (`#223`, TDD) — enum `DashboardRange { Week, Month, All }`; `GetDashboardSummaryQuery`
  bierze `Range`; `GET /api/dashboard?range=` binduje (default All). Handler skopuje score-over-time,
  category strength, recent activity, totals i average do zakresu; **streak + sparkline zostają
  all-time**. 5 nowych testów handlera (granica Week/Month, All bez filtra, streak/sparkline nietknięte,
  pusty zakres przy danych all-time). Application **141/141**, Api **23/23**.
- **Frontend** (`#224`) — `dashboardKey(range)` w kluczu, `useDashboard(range)` +
  `fetchDashboardSummary(range)` wysyłają `?range=`. `keepPreviousData` trzyma poprzedni zakres na
  ekranie podczas przełączania (bez flasha loadingu). Segmented control w headerze (oba stany), default
  All. First-run prompt tylko gdy `range === 'all' && averageScore === null`.

**Decyzje:**
- **Streak i sparkline poza filtrem** — to „stan na teraz" (kolejne dni do dziś / stałe okno 14 dni),
  Week/Month nie powinien ich przepisywać (inaczej „streak: 0" w poniedziałek rano na widoku „Week").
- **Okna date-based, UTC** — Week = dziś−6 (7 dni), Month = dziś−29 (30 dni), spójnie ze streak.
- **Bez zmiany DTO na empty-state** — `averageScore` odnosi się do zakresu; na „All" null = brak
  jakichkolwiek prób → first-run. Węższy pusty zakres przy danych all-time → populated layout z pustymi
  wykresami, nie prompt.
- **API binding** — `range` po nazwie (case-insensitive); brak → default All; śmieci → 400 (frontend
  i tak wysyła tylko week/month/all).

**Weryfikacja:**
- `pnpm build` (tsc) + `pnpm lint` czyste.
- Stack postawiony (rebuild api/web), demo login, przełączanie zakresów (agregaty się zmieniają,
  streak/sparkline trzymają), oba motywy — owner potwierdza w przeglądarce.

---

## 2026-06-30 — Iteracja 2.2: Dashboard UI

**Cel:** Dashboard screen — 8-kaflowy bento grid z `mockups/dashboard-*.html`, zasilany jednym
`GET /api/dashboard` (kontrakt z 2.1). W obu motywach. Promocja „Dashboard" z disabled „soon" do
żywej trasy.

**Co zrobione (plan + 3 atomic commits):**
- **Plan** (`#217`) — pełny plik iteracji 2.2 (cel, scope, DoD, TDD task list).
- **Data layer** (`#218`) — `features/dashboard/`: `api.ts` (typy lustrzane do `DashboardSummaryDto`
  + `fetchDashboardSummary`), `query-keys.ts`, `use-dashboard.ts` (TanStack **query**, nie mutacja —
  cached read keyed per sesja).
- **Screen + route** (`#219`) — `dashboard-page.tsx`: 8 kafli z jednego `useDashboard()`, stany
  loading/error/empty. **Recharts** AreaChart (score over time) + RadarChart (category strength);
  sparkline jako inline SVG (bez dep). Best/Needs-practice liczone po stronie klienta (max/min z
  `categoryStrength`). Empty-state gdy `averageScore === null`. `/dashboard` route + promocja nav
  z `COMING_SOON` do aktywnego `NavLink`.

**Decyzje:**
- **Nowa zależność: Recharts** (hard rule #6) — line + radar uzasadniają chart lib już nazwaną w
  stack table jako Phase-2 charting. Sparkline zostaje hand-rolled (pojedynczy sub-100px path tańszy
  bez biblioteki).
- **Jeden query, wszystkie kafle** — strona robi jeden `useDashboard()` i rozdaje payload do kafli;
  bez per-tile fetchy. Dokładnie po to 2.1 zwraca jeden agregat.
- **Bez fabrykowanych delt** („+34 w tym miesiącu", „▲ 4%") — wymagają danych time-range (2.3).
  Zostaje prawdziwy trend score-over-time (pierwszy vs ostatni punkt).

**Weryfikacja:**
- `pnpm build` (tsc) + `pnpm lint` czyste.
- Stack postawiony (rebuild api/web), demo login, oba motywy + empty/populated — owner potwierdza
  w przeglądarce.

---

## 2026-06-30 — Iteracja 2.1: Dashboard data layer

**Cel:** read-side pod Dashboard — jeden endpoint `GET /api/dashboard` zasilający wszystkie 8 kafli
bento grid w jednym round-tripie. Bez UI (to 2.2).

**Co zrobione (plan + 4 atomic commits):**
- **Plan** (`#211`) — rozbicie outline'u sprintu 2 na pełny plik iteracji 2.1 (cel, kontrakt, DoD,
  TDD-first task list).
- **Repo read** (`#212`) — `IQuizRepository.GetCompletedAttemptsWithCategoryAsync`: spłaszcza
  ukończone, ocenione próby usera z nazwą kategorii i liczbą odpowiedzi, oldest→newest po
  `CompletedAt`. Projekcja `CompletedAttemptRow`. 2 testy integracyjne (Testcontainers): scoping do
  usera + wykluczenie in-progress + kolejność, oraz translacja `Answers.Count` do SQL.
- **Use case** (`#213`) — `GetDashboardSummaryQuery` + handler (TDD, mock repo) → `DashboardSummaryDto`.
  Agregacja w pamięci: streak, sparkline 14 dni, score-over-time, category strength, suma odpowiedzi,
  średnia, recent activity, oraz ścieżka empty-state. 10 testów.
- **API** (`#214`) — `GET /api/dashboard` przez MediatR, `[Authorize]`, scoped do `IUserContext.UserId`.
  2 testy smoke (`WebApplicationFactory`): 401 bez tokenu, 200 z tokenem.

**Decyzje:**
- **Jeden endpoint, nie osiem.** Dashboard renderuje się jako całość — jeden round-trip, jeden cache
  key pod TanStack Query w 2.2. Filtr czasowy (2.3) dołoży query param do tej samej trasy, nie nowe
  routy.
- **Agregacja w pamięci, nie w SQL.** Zbiór ukończonych prób jednego usera jest mały; streak/średnie
  liczone w handlerze są testowalne z mockowanym repo i nie wymuszają przedwczesnych read modeli
  (pragmatyzm ADR-013). Do rewizji dopiero gdy 2.x pokaże problem perf.
- **Streak: date-based, UTC, z jednodniowym grace.** „Kolejne dni z ukończoną próbą, licząc wstecz od
  dziś". Jeśli dziś jeszcze nie grano, liczymy od wczoraj — niezagrany *dzisiaj* nie zrywa serii, dopóki
  nie minie pełna doba bez aktywności. Granica doby = UTC (deterministyczne testy). Udokumentowane w
  nazwach testów (`...EndingToday`, `...HasOneDayGrace`, `...BreaksOnAGap`).
- **Best / Needs-practice bez osobnych pól.** To max/min z `categoryStrength` — UI w 2.2 wybierze
  skrajne, DTO nie dubluje agregacji.
- **Empty-state w kontrakcie.** Brak ukończonych prób → `averageScore: null`, puste listy,
  `currentStreakDays: 0`, sparkline 14×0 — 2.2 wyrenderuje empty-state z tego samego payloadu.

**Weryfikacja:**
- Pełny pakiet solucji zielony: Domain **81/81**, Application **136/136** (w tym 10 dashboard),
  Infrastructure **45/45** (w tym 2 nowe repo), Api **23/23** (w tym 2 smoke). 0 niepowodzeń.
- Bez kroku w przeglądarce — w tej iteracji nie ma UI; widoczna weryfikacja przyjdzie w 2.2 (Dashboard
  UI). Status 2.1 → **done**.
