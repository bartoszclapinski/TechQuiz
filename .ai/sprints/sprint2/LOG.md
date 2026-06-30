# Sprint 2 — Log

Chronologiczny dziennik pracy w sprincie 2 (Phase 2: Dashboard + Spaced Repetition).
Najnowsze wpisy na górze.

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
