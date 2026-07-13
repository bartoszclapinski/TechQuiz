# Sprint 4 — Log

Chronologiczny dziennik pracy w sprincie 4 (Phase 4: Polish & Deployment).
Najnowsze wpisy na górze.

---

## 2026-07-13 — Iteracja 4.7: Final polish (domknięcie Phase 4)

**Cel:** ostatnie szlify portfolio-facing po redesignie/gamifikacji/a11y/perf. Trzy slice'y, PR na slice.

- **4.7.1 (#359)** — **SEO / social meta**: `index.html` dostaje meta description, Open Graph + Twitter
  card, `theme-color` i opisowy `<title>` — udostępniony link demo ładnie się podgląda i czyta jak
  realny produkt.
- **4.7.2 (#361)** — **README refresh**: banner z live URL + „Continue as demo", intro pod Momentum +
  gamifikację, tech-stack (bez recharts, + system typograficzny i code-splitting), sekcja Features pod
  faktycznie dostarczone Phase 2/3/4, nota o zamkniętej rejestracji i samo-odświeżającej się historii demo.
- **4.7.3 (#363)** — **a11y tail polish**: dekoracyjne glify w tekście (👋 👏 🎉 🔥 → ✓) owinięte w
  `aria-hidden` (landing/login/dashboard/result/categories/generate), żeby SR je pomijał. Ikony ✓/✗
  per-test w code-challenge dostają alternatywę tekstową (`role=img` + aria-label Passed/Failed) — kolor
  był jedynym sygnałem, co łamało WCAG 1.4.1. `role=progressbar` z value/min/max na pasku Level/XP oraz
  na `ProgressBar` kategorii. Stany loading/error/empty potwierdzone jako spójne po redesignie (wspólny
  `text-[15px] text-secondary`, wyśrodkowane) — bez dodatkowego refaktoru.

**Weryfikacja:** `pnpm build` + `pnpm lint` czyste na każdym slice; główny chunk bez zmian (381/115 kB gzip).
**Phase 4 zamknięta** — roadmap dostarczona (deploy 4.6, mobile 4.1, redesign 4.10, demo 4.11, gamifikacja
4.2, a11y 4.4, perf 4.5, final 4.7).

---

## 2026-07-10 — Iteracja 4.5: Performance (bundle)

**Cel:** zmniejszyć initial bundle (Render free tier + cold starty bolą). Cała appka szła jako jeden
~591 kB chunk (CI ostrzegał >500 kB), bo wszystkie strony były importowane eager w `App.tsx`.

- **4.5.1 (#353)** — **route-based code splitting**: każda strona autoryzowana jako `React.lazy` +
  `Suspense` wokół Outletu w `AppShell` (obie ścieżki — z chrome i focused runner quiz/review). Landing,
  Login i layouty zostają eager (entry points). Główny chunk **591 → 381 kB (175 → 115 kB gzip)**;
  każda strona to osobny chunk na żądanie (dashboard 21, quiz 39, code-challenge 22, result 10 kB…).
  Ostrzeżenie >500 kB zniknęło.
- **4.5.2 (#355)** — usunięty martwy `recharts` (wykresy zastąpione przy redesignie/gamifikacji; już
  wytree-shakowany, ale wisiał w `package.json`).

**Uwaga:** Monaco ładuje core runtime'owo (loader `@monaco-editor/react`) — nigdy nie był w bundlu.
Vendor-chunk splitting rozważony i pominięty (marginalny dla portfolio; splitting tras i tak usunął ostrzeżenie).

---

## 2026-07-10 — Iteracja 4.4: Audyt dostępności (a11y, WCAG AA)

**Cel:** doprowadzić appkę po redesignie Momentum do solidnego poziomu WCAG 2.1 AA (nie-opcjonalne wg roadmapy).

- **4.4.1 (#345)** — **focus klawiatury**: globalny `:focus-visible` (akcentowy outline) — custom-stylowane
  buttony/pille/karty-buttony gubiły domyślny focus. + **skip-link** „Skip to content" i focusowalny `#main-content`.
- **4.4.2 (#347)** — **kontrast**: audyt całej palety (policzone ratio). Trzy tokeny nie przechodziły AA dla
  małych tekstów → podbite z zachowaniem odcienia: dark muted `#7d7291→#948aac`, light muted `#9a8fb5→#736a88`,
  light amber `#d97706→#a85400`. **ADR-026** (bo ADR-024 mówił „tokeny verbatim"). Wyjątek: gradient przycisku
  (biały tekst, large/bold → próg 3:1, spełnia).
- **4.4.3 (#349)** — **semantyka/ARIA** (z pełnego audytu komponentów subagentem): weekly-bary miały wartość
  tylko w `title` → `role=img` + aria-label per-dzień; brak `<h1>` na runnerach quiz/review → pytanie jako `<h1>`;
  dekoracyjne emoji (features Landingu, kafle review/empty dashboardu) → `aria-hidden`; przeskoki nagłówków
  (karty categories/history/empty h3→h2); `role=progressbar` na pasku quizu.

**Audyt potwierdził brak problemów** w: nazwach kontrolek icon-only, klikalnych nie-semantycznych elementach,
obrazkach/awatarach (brak `<img>`, wszystko inline SVG `aria-hidden` / inicjały).

---

## 2026-07-10 — Iteracja 4.2: Gamifikacja (XP / poziomy / Skill IQ)

**Cel:** ożywić metryki, na które redesign Momentum zostawił miejsca (XP, poziom, Skill IQ) — realnie,
z danych które już mamy. Decyzja: **ADR-025**. Owner: „skończmy całość i pójdźmy z gamifikacją; Skill IQ
w panelu logowania zostaw aż skończymy".

**Podejście derive-on-read** (jak `AchievementCalculator`): zero zmian schematu, zero persystowanych
liczników — wszystko funkcją ukończonych podejść, więc nie może się rozjechać z danymi.

- **4.2.1 (#337)** — Domain: `Gamification` (czysta matematyka, 24 testy). XP = `correctCount×10`;
  krzywa poziomu `100+(L-1)×50` → poziom + postęp; Skill IQ = `clamp(avg,0..100)×1.6 + min(quizCount×3,75)`
  (0..250); tier zamiast fikcyjnego percentyla „Top X%".
- **4.2.2 (#339)** — Application: `GamificationCalculator` nad `CompletedAttemptRow` → `GamificationDto`
  (totalXp, level, xpIntoLevel, xpForNextLevel, skillIq, weeklyDelta, tier). Dodany do `DashboardSummaryDto`
  jako blok **all-time** (jak streak — filtr zakresu go nie tyka), liczony w handlerze z podejść, które i tak
  czyta. `QuizResultDto` zyskuje `XpEarned`. correctCount odtwarzany z `round(score% × answerCount)`.
- **4.2.3 (#341)** — Web: Dashboard hero = **Skill IQ** (wartość + delta tygodnia + tier + pasek Level/XP),
  obok karta **Accuracy**; wykres score-over-time usunięty (nie ma go w mockupie, znika Recharts z tego ekranu).
  Result: kafelek **XP earned**. Quiz: amber pill **potencjalnego XP** (pytania×10) w górnym pasku.

**Weryfikacja na żywo (demo):** `/api/dashboard` → `skillIq:181` (Advanced), `level:9`, `totalXp:2370`,
`skillIqWeeklyDelta:48`. ✅ Skill IQ w panelu logowania (marketing pre-login) świadomie zostaje statyczny.

---

## 2026-07-09 — Iteracja 4.11: Demo hardening (historia demo + zamknięcie rejestracji)

**Cel:** żywe demo ma być samo-tłumaczące i bezpieczne do zostawienia otwartego.

**Kontekst (owner):** (1) świeżo zalogowany gość trafiał na pusty dashboard — słabe na portfolio;
(2) owner nie chce, żeby ktoś się rejestrował, bo nie ma jeszcze polityki prywatności/RODO ani sposobu
na zwrot/usunięcie danych użytkownika. Dwie decyzje: świeżość demo = **odśwież przy każdym starcie**;
rejestracja = **endpoint 403 + ukryte UI**.

- **4.11.1 (#331)** — `DataSeeder.SeedDemoHistoryAsync`: ~19 ukończonych podejść w 8 kategoriach przez
  ostatnie ~14 dni (streak ~12, wyniki lekko rosnące, spójne odpowiedzi). **Nie-idempotentne z rozmysłem** —
  kasuje podejścia demo i tworzy od nowa z datami względem „teraz" przy każdym boocie, więc demo nigdy się
  nie starzeje i konto samo się sprząta. Ruszany tylko demo user. Testy: seeduje ukończoną historię +
  odświeżenie nie kumuluje podejść.
- **4.11.2 (#333)** — flaga `Auth:RegistrationEnabled` (bazowo **false** → Staging/live zamknięte; **true**
  w `appsettings.Development` → lokalnie i testy integracyjne dalej rejestrują). `/api/auth/register` zwraca
  403 ProblemDetails zanim powstanie jakikolwiek user. Web: `/register` → redirect na `/login`, link „Create
  one" zastąpiony notką „demo only", CTA Landingu → `/login`. Odwracalne flagą, gdy powstanie polityka prywatności.

**Weryfikacja na żywo:** `POST /api/auth/register` → 403 „Registration closed"; demo login → `/api/dashboard`
zwraca `currentStreakDays:12` + wypełniony sparkline + rosnący `scoreOverTime`. ✅

---

## 2026-07-08 — Iteracja 4.10: Redesign „Momentum"

**Cel:** wdrożyć nowy system designu z handoffu (`.ai/design/…/TechQuiz Momentum.dc.html`) — violet + amber,
Bricolage Grotesque, oba motywy. Decyzja: **ADR-024**. Owner dostarczył wysokiej wierności prototyp HTML.

**Dwie decyzje ownera na starcie:** (1) gamifikacja bez pokrycia (XP/poziomy/Skill IQ) — **realne metryki teraz,
XP jako osobna iteracja backendowa później** (nie fejkujemy proxy); (2) Landing — **robimy, ale na końcu**.

**Podejście token-first:** appka stała na semantycznych tokenach CSS mapowanych przez Tailwind, więc podmiana
wartości przeskórowała **wszystkie** ekrany naraz (też te spoza mockupu). Dostarczone po jednym PR na slice:

- **4.10.1 (#305)** — fundament: tokeny Momentum (dark+amber / light) + Bricolage + utility Tailwinda
  (`font-display`, `amber`, `bg-brand/btn/card-grad`, `shadow-float/focus`, `rounded-pill`).
- **4.10.2 (#307)** — chrome: header (gradientowe logo, nav-pills, gradientowy avatar, `max-w-[1560px]`).
- **4.10.3 (#309)** — Dashboard bento na realnym `DashboardSummary`: hero „Average score" (glow + trend),
  Streak, Questions, paski kategorii, słupki tygodnia (z sparkline), recent. Radar → paski (jak mockup).
- **4.10.4 (#311)** — Categories: karty Momentum nad taksonomią Track→Category, „Soon/Not started" dla pustych.
- **4.10.5 (#313)** — Quiz: pytanie Bricolage, opcje z gradientowym badge na zaznaczeniu, amber difficulty.
  **Bez natychmiastowego reveala** poprawnej odpowiedzi — API nie zwraca `IsCorrect` w aktywnym quizie (hard rule #4).
- **4.10.6 (#315)** — Result: okrągły badge %, kafelki nagród (realne: correct/time/vs-last), review zachowane.
- **4.10.7 (#317)** — Auth: gradientowe logo, Bricolage, inputy 14px + focus-ring, panel marketingowy (glow,
  floating card, stat-tiles, testimonial, social-proof).
- **fix (#319)** — Login był przyklejony do lewej → panel info na lewo, formularz na prawo i wyśrodkowany (feedback ownera).
- **4.10.8 (#…)** — Landing: publiczny `/`, hero + features + „how it works" + topics + footer. Copy uczciwe
  (streak/accuracy/progress — realne pojęcia; bez twardych fake liczb XP/Skill IQ).

**Guardrails:** tylko realne dane na ekranach z danymi użytkownika; istniejący `ThemeProvider` (nie localStorage
z prototypu); wszystkie stany zachowane (review-banner, achievements, empty-state, keyboard w quizie).

---

## 2026-07-05 — Iteracja 4.8: Taksonomia kategorii (Tracks) + czyszczenie treści

**Cel:** zamienić płaską listę 9 kategorii na dwupoziomową taksonomię **Track → Category (quiz) →
Question** i usunąć z treści atrybucję do zewnętrznego kursu. Decyzja: **ADR-023**.

**Kontekst (zgłoszone przez ownera na żywym deployu):** (1) opisy kategorii cytowały nazwę kursu — na
portfolio niedopuszczalne; (2) 9 płaskich kafli bez grupowania; (3) grube banki (SQL/Front-End/Engineering)
mieszały wiele podtematów w jednym quizie. Owner wybrał **pełne rozbicie na podtematy** (nie tylko grupowanie).

**Taksonomia:** 4 tracki nad 18 podkategoriami:
- **.NET** — C#/.NET · ASP.NET Core · EF Core · ADO.NET · Unit Testing · Design Patterns (6 bez zmian).
- **Databases** — SQL(30) rozbite na Database Fundamentals · Normalization · Querying · Data Manipulation · Schema Definition.
- **Front-End** — 30 rozbite na JavaScript · Async & Events · TypeScript · HTML & CSS.
- **Engineering Practices** — 30 rozbite na Git & Version Control · CI/CD · Clean Code.
- **Practical Challenges** — nesting tylko we froncie (nie Track w bazie), link do `/challenges`; wypięte z topnav.

**Kluczowy zabieg (zero ryzyka contentowego):** rozbicie to **przepartycjonowanie istniejących metod-fabryk
pytań** — teksty/opcje/wyjaśnienia bajt-w-bajt bez zmian; zmienia się tylko która podlista je referuje.
Suma pytań bez zmian: **269** (i 1076 opcji). Invariant single-correct zachowany.

**Co zrobione (6 atomic commitów, issues #293–298):**
- **docs** (`#293`) — ADR-023 + plik iteracji 4.8.
- **domain** (`#294`) — encja `Track` + `Category.TrackId`/`Position` (TDD).
- **infra** (`#295`) — tabela `tracks`, FK `categories.track_id` (cascade), migracja `AddTracks`, `GetTracksAsync`.
- **api** (`#296`) — `TrackDto` (zagnieżdżone `CategoryDto`), handler grupuje po tracku wg pozycji, kontroler zwraca tracki.
- **seed** (`#297`) — reorg `DataSeeder` na tracki+podkategorie, rozbicie 3 banków, usunięcie atrybucji z opisów i komentarzy.
- **web** (`#298`) — strona kategorii jako master/detail (kafle tracków → drill do podkategorii), kafel Practical Challenges, spłaszczenie tracków dla filtra History, wypięcie Challenges z topnav.

**Weryfikacja:** cały suite .NET zielony — **Domain 105, Application 196, Infrastructure 60, Api 37 = 398**
(w tym integracyjny full quiz-flow przez nowy kształt tracków). Web `pnpm build` + `pnpm lint` czyste.
`grep EPAM` po `src/` — czysto.

**Uwaga deployowa (do Fazy C):** migracja `AddTracks` doda `track_id`=pusty-Guid do istniejących 9 kategorii
na Neonie i FK to wywali na starcie. Dlatego przed deployem **reset tabel treści na Neonie** (kategorie/quizy/
pytania/opcje) — konta userów nietknięte — żeby nowa taksonomia zaseedowała się od zera. Status iteracji
`in-progress` do potwierdzenia klik-through na żywym URL-u.

---

## 2026-07-02 — Iteracja 4.1: Mobile responsive

**Cel:** każdy ekran ma być używalny na telefonie (**375–768px**) bez poziomego scrolla, ściśniętego
chrome'u i nieosiągalnych akcji. Punkt startowy: topbar upychał logo + **7** linków nav +
settings/theme/avatar w jednym rzędzie (rozjeżdżało się grubo przed 768px), a runnery miały stopkę z
podpowiedzią klawiaturową tłoczącą przycisk. Czysty frontend — **zero zmian w backendzie/API/kontraktach**.

**Kluczowe ustalenie:** aplikacja była **już w dużej mierze responsywna** — była budowana z breakpointami
`sm:`/`lg:` od początku (categories, dashboard, history, generate, settings, pool, edytor kodu już
reflow-ują do jednej kolumny, edytor jest ułożony pionowo). Realne luki to **topbar** i **stopka
runnerów**. Zamiast produkować puste commity „dla porządku", zakres kodu ograniczył się do tych dwóch
miejsc; resztę potwierdzono click-through'em.

**Co zrobione (plan + 2 atomic commits kodu):**
- **Plan** (`#269`) — plik iteracji 4.1; decyzje: (a) mobile-first, breakpointy Tailwind; (b) `md`
  (768px) jako próg nawigacji; (c) zero zmian backend/API/route; (d) edytor kodu **stackowany**, nie
  wymyślany od nowa na mobile (i tak już jest pionowo); (e) brak testów komponentowych (reguła MVP);
  (f) galeria odznak/toasty z 4.3 poza zakresem — 2.9 dostarczyło sekcję, 4.1 tylko dba o jej reflow.
- **Responsive nav shell** (`#270`) — `AppShell`: wspólna lista `NAV_ITEMS` renderowana dwojako —
  poziomy pasek na `md+` i **hamburger → wysuwany drawer** poniżej `md` (linki + Settings + Log out,
  rzędy ≥44px, tło zamyka na tap, blokada scrolla body, zamykanie na tap linku). Runnery
  (`/quiz/:id`, `/review/run`) dalej bez chrome'u (ADR-014).
- **Mobile polish runnerów** (`#271`) — podpowiedź klawiaturowa (1-4 / Enter) ukryta poniżej `sm`
  (telefony nie mają tych skrótów), przycisk „Next/Submit" dosunięty do prawej na mobile.

**Ekrany potwierdzone jako już responsywne (bez zmian):** categories (grid 1→2→3), history
(`flex-col sm:flex-row`, `flex-wrap` na kontrolkach), dashboard (bento `sm:grid-cols-3`, Recharts w
`ResponsiveContainer`), generate/settings/pool (`max-w-3xl px-6 sm:px-9`), edytor code-challenge
(edytor pełnej szerokości → kontrolki → wyniki, pionowo).

**Testy/build:** `pnpm build` + lint czyste na każdym commicie (naprawiony 1 błąd lint — `setState` w
efekcie zamieniony na `onClick={onClose}` na linkach drawera).

**Weryfikacja:** dev server Vite na :5173 + API w dockerze na :8085 (login demo 200). Click-through
właściciela na 375/414/768px, oba motywy: hamburger drawer działa (otwiera/zamyka, nawigacja, blokada
scrolla), brak poziomego scrolla na wszystkich ekranach, runnery czytelne. Potwierdzone („działa
elegancko").

**Świadomie odpuszczone (zgodnie z planem):** gamifikacja XP/levele (4.2), galeria odznak/toasty (4.3),
audyt a11y / focus-trap (4.4), performance/Lighthouse (4.5), tuning pod landscape/tablet.

**Następne wg planu:** pozostałe iteracje Phase 4 — 4.4 (a11y) i 4.6 (deployment) to elementy
nie-opcjonalne; 4.2/4.3 (gamifikacja) opcjonalne, częściowo pokryte przez 2.9.
