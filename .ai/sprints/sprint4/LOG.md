# Sprint 4 — Log

Chronologiczny dziennik pracy w sprincie 4 (Phase 4: Polish & Deployment).
Najnowsze wpisy na górze.

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
