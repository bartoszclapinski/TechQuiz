# TechQuiz — Plan roboczy (PL)

> **To jest prywatny dokument roboczy.** Trzymany w `Personal-Agent/Portfolio_Website/`, nigdy nie idzie do public repo.
> Publiczne dokumenty projektu (po angielsku): `F:\Repos\TechQuiz\README.md`, `docs/ARCHITECTURE.md`, `docs/DECISION_LOG.md`.

> **Status:** Decyzje strategiczne zamknięte 2026-05-11. Gotowy do Fazy 0.
> **Stack:** ASP.NET Core 9 + React/TS + PostgreSQL + AI (multi-provider)

---

## DECYZJE STRATEGICZNE — szybki przegląd

| Decyzja | Wybór |
|---------|-------|
| Nazwa projektu | TechQuiz (finalna) |
| Język projektu | English-only (kod, UI, commits, docs, issues) |
| Multi-user | Tak, Identity + JWT, bez reset/email confirmation w MVP |
| Frontend | React 18 + TypeScript + Vite |
| Backend | ASP.NET Core 9 + EF Core + PostgreSQL |
| AI | Multi-provider (`IAiProvider`), klucze per user szyfrowane, public pool z moderacją |
| TDD | Domain + Application layer |
| Hosting bazy | PostgreSQL w Dockerze (Firebase odrzucone) |
| Scope | MVP w 3 miesiące, reszta iteracyjnie |
| Commits | Conventional Commits bez scope + commitlint |
| Branch strategy | GitHub Flow + feature branches + PR + squash merge |
| Logging | Serilog → Console + Seq w docker-compose |
| Aesthetic | Premium SaaS dark mode, accent violet-500, bento w Fazie 2 |

Pełne uzasadnienia każdej decyzji: `docs/DECISION_LOG.md` (ADR-001 do ADR-013).

---

## LOKALIZACJA KODU

| Co | Gdzie |
|----|-------|
| **Kod aplikacji (solution, projekty, testy)** | `F:\Repos\TechQuiz\` |
| **GitHub** | Private repo na start, public po Fazie 1 |
| **Polski plan roboczy (ten plik)** | `Personal-Agent\Portfolio_Website\TechQuiz_Plan_Roboczy_PL.md` |
| **Publiczne dokumenty (EN)** | `F:\Repos\TechQuiz\README.md`, `docs/ARCHITECTURE.md`, `docs/DECISION_LOG.md` |

**Dlaczego osobny katalog:** Personal-Agent ma prywatne dane (CV, plany kariery) — NIGDY nie idzie publiczne. TechQuiz to projekt portfolio z własną historią gita.

---

## STRUKTURA SOLUTION

```
F:\Repos\TechQuiz\
├── TechQuiz.sln
├── src/
│   ├── TechQuiz.Domain/              # POCO, encje, interfejsy, brak zależności
│   ├── TechQuiz.Application/         # MediatR handlers, services, validators
│   ├── TechQuiz.Infrastructure/      # EF Core, Identity, AI providers
│   └── TechQuiz.API/                 # ASP.NET Core Web API
├── tests/
│   ├── TechQuiz.Domain.Tests/        # TDD core
│   └── TechQuiz.Application.Tests/   # TDD handlers
├── client/                           # React + TS (osobno, NIE w solution)
├── docs/
│   ├── ARCHITECTURE.md
│   └── DECISION_LOG.md
├── docker-compose.yml                # PostgreSQL + Seq
├── commitlint.config.js
├── package.json                      # tylko dla commitlint + husky
├── .gitignore
└── README.md
```

**Uwaga:** React projekt w `client/`, poza solution. `dotnet build` nie powinno triggerować `npm install`. Dwa osobne worldy, łączone tylko w Dockerze i CI.

---

## FAZY BUDOWANIA

### FAZA 0 — Setup (1 sesja)

**Cel:** Działający szkielet z auth. Stan końcowy: można zarejestrować usera przez Swaggera i dostać JWT.

#### Tasks:
- [ ] `git init` w `F:\Repos\TechQuiz\`
- [ ] `dotnet new sln -n TechQuiz`
- [ ] 4 projekty src + 2 testowe (`dotnet new classlib/webapi/xunit`)
- [ ] Dodanie referencji między projektami (Application → Domain, Infrastructure → Application, API → Application + Infrastructure)
- [ ] NuGet packages dla każdego projektu:
  - Application: MediatR, FluentValidation
  - Infrastructure: EF Core, Npgsql, Identity, JWT Bearer
  - API: Serilog.AspNetCore, Serilog.Sinks.Seq, Swashbuckle
  - Tests: xUnit, FluentAssertions, NSubstitute
- [ ] `npm init -y` + commitlint + husky w roocie
- [ ] `commitlint.config.js` z `@commitlint/config-conventional`
- [ ] Husky hook `commit-msg` z `commitlint --edit`
- [ ] `docker-compose.yml` z PostgreSQL 16 + Seq
- [ ] `docker compose up -d` — sprawdź czy baza i Seq startują
- [ ] `ApplicationUser : IdentityUser` w Domain
- [ ] `AppDbContext : IdentityDbContext<ApplicationUser>` w Infrastructure
- [ ] Connection string w `appsettings.Development.json`
- [ ] `dotnet ef migrations add InitialIdentity` + `database update`
- [ ] JWT setup w `Program.cs` (config, middleware)
- [ ] `AuthController` z `Register` i `Login`
- [ ] Pierwszy test TDD: `ApplicationUserTests` (trivial, na zielono)
- [ ] Integration test: register + login flow z `WebApplicationFactory`
- [ ] GitHub Actions workflow (build + test on PR + push to main)
- [ ] Branch protection rules na `main`:
  - Require PR before merging
  - Require status checks to pass
  - Automatically delete head branches
- [ ] Repo settings: tylko squash merge, wyłącz merge commits i rebase
- [ ] `.gitignore` (bin, obj, .vs, node_modules, logs, .env)
- [ ] README skeleton + pierwsze commity (`chore: initial solution structure`, kolejne przez PR)
- [ ] Push do GitHuba (private repo)

#### Definicja "done" dla Fazy 0:
- ✅ `dotnet build` przechodzi
- ✅ `dotnet test` przechodzi (≥2 testy)
- ✅ Możesz zarejestrować usera przez Swaggera
- ✅ Login zwraca JWT
- ✅ Seq pokazuje logi rejestracji i logowania
- ✅ Commit message bez `feat:`/`fix:`/etc. → odrzucony przez commitlint
- ✅ Direct push do `main` → odrzucony przez branch protection
- ✅ GitHub Actions zielone na pierwszym PR

---

### FAZA 1 — MVP

**Cel:** Pełny flow quizu end-to-end. Po Fazie 1 projekt jest showable w CV.

#### 1A. Domain + Application Layer (TDD)
- [ ] Test + implementacja: `Category` entity
- [ ] Test + implementacja: `Question` entity (typ, trudność, walidacje)
- [ ] Test + implementacja: `Answer` entity
- [ ] Test + implementacja: `QuizAttempt` entity (start, complete)
- [ ] Test + implementacja: `QuizResponse` entity
- [ ] Test + implementacja: `QuizScoringService`
- [ ] MediatR commands: `StartQuizCommand`, `SubmitAnswerCommand`, `CompleteQuizCommand`
- [ ] MediatR queries: `GetCategoriesQuery`, `GetQuizResultQuery`
- [ ] FluentValidation walidatory dla każdego command
- [ ] Application tests dla handlerów (NSubstitute dla repo)

#### 1B. Infrastructure + API
- [ ] EF Core entity configurations (`IEntityTypeConfiguration<T>`)
- [ ] Migracja `AddQuizDomain`
- [ ] Repozytoria (`ICategoryRepository`, `IQuestionRepository`, `IQuizAttemptRepository`)
- [ ] Seed data: 10-20 questions C# Basics + ASP.NET Core
- [ ] Demo user w seed: `demo@techquiz.dev` / `Demo123!` + 3 historyczne attempts
- [ ] Controllers: `CategoriesController`, `QuizzesController`
- [ ] Authorization `[Authorize]` wszędzie poza `/api/auth/*`
- [ ] Manual test przez Swaggera: pełny flow

#### 1C. React Frontend
- [ ] `npm create vite@latest client -- --template react-ts`
- [ ] Tailwind setup + tokeny (slate-950, violet-500, Inter)
- [ ] React Router v6 setup
- [ ] Axios instance + JWT interceptor + token w localStorage
- [ ] Login page + Register page
- [ ] `useAuth` hook + `<RequireAuth>` wrapper
- [ ] Categories page (lista z API)
- [ ] TanStack Query setup
- [ ] Quiz flow page (jedno pytanie na ekran, progress bar)
- [ ] Result page (test mode — feedback dopiero tutaj)

#### Definicja "done" dla Fazy 1:
- ✅ Pełny flow end-to-end w UI: register → login → categories → quiz → result
- ✅ Test mode: odpowiedzi widzisz dopiero na końcu
- ✅ Scoring liczy się poprawnie
- ✅ Demo user widoczny z historią
- ✅ README z screenshotami i demo GIF
- ✅ Repo gotowe do przejścia na public

---

### FAZA 2 — Dashboard + Spaced Repetition

**Cel:** Wykresy, historia, inteligentne powtórki.

- [ ] Domain: `UserProgress` entity + aggregation logic
- [ ] Application: `UpdateProgressAfterQuizCommand` (event-driven po complete)
- [ ] API: GET endpoints na progress, history, stats per category
- [ ] React: Dashboard z **bento grid layout** (Recharts dla wykresów)
  - Tile: streak counter
  - Tile: wykres liniowy wyników w czasie
  - Tile: wykres radarowy siły w kategoriach
  - Tile: recent attempts (lista)
  - Tile: achievements (placeholder dla Fazy 4)
- [ ] React: History page z filtrami i sortowaniem
- [ ] Domain: SM-2 simplified spaced repetition algorithm
- [ ] API: GET `/api/quizzes/review` — pytania do powtórki
- [ ] React: Daily Review mode

---

### FAZA 3 — AI Integration

**Cel:** AI generuje pytania, ocenia kod, obsługa kluczy per user.

- [ ] Domain: rozszerzenie `Question` o pola AI (`IsAIGenerated`, `Provider`, `GeneratedByUserId`, `ApprovalStatus`)
- [ ] Domain: `CodeQuestion` (snippet, expected output, explanation)
- [ ] Domain: `UserAiKey` entity (encrypted blob)
- [ ] Application: `IAiProvider` interface
- [ ] Infrastructure: `OpenAiProvider` implementation
- [ ] Infrastructure: `AnthropicProvider` implementation
- [ ] Infrastructure: `EncryptedAiKeyVault` z `IDataProtectionProvider`
- [ ] Application: `QuestionGenerationService` (cache check → provider call → save → return)
- [ ] Application: `CodeEvaluationService`
- [ ] API: POST `/api/users/me/ai-keys` (zapis klucza)
- [ ] API: POST `/api/quizzes/generate` (AI tworzy quiz)
- [ ] API: POST `/api/questions/{id}/evaluate-code` (AI ocenia odpowiedź)
- [ ] Quality control: schema validation dla AI output
- [ ] Community voting: POST `/api/questions/{id}/vote`
- [ ] Rate limiting per user (np. 20/h)
- [ ] React: Settings page — wprowadzanie kluczy AI
- [ ] React: Monaco Editor integration dla code questions
- [ ] React: Generate quiz UI (wybór tematu, trudności, provider)
- [ ] React: Feedback UI dla code evaluation

---

### FAZA 4 — Polish + Deployment

**Cel:** Gamifikacja, deployment, kompletne portfolio.

- [ ] Domain: `Badge` entity + condition logic
- [ ] Application: `BadgeUnlockingService` (sprawdza warunki po attempt)
- [ ] React: Profile page z achievements
- [ ] React: XP + Level system w UI
- [ ] Refresh tokens (zamiana 24h JWT na refresh flow)
- [ ] Email service (SendGrid) — confirmation + password reset
- [ ] Integration tests dla całego API (Testcontainers)
- [ ] Frontend tests (Vitest + RTL)
- [ ] Docker: pełna konteneryzacja (API + React + DB + Seq w jednym docker-compose)
- [ ] Deployment na Railway / Render / Azure (decyzja w trakcie Fazy 4)
- [ ] CI/CD: deploy na staging po merge do main
- [ ] README polish: screenshots, demo GIF, badges
- [ ] LinkedIn post o projekcie

---

## CO ZOSTAŁO ODŁOŻONE NA PO FAZIE 4

Te rzeczy są wymienione w pierwotnym planie ale nie wchodzą w 4 fazy:

- Multi-user z globalnym rankingiem
- Import pytań z JSON/CSV
- Tryb "Interview Simulation" (30 min, losowe pytania, timer)
- Integracja z GitHub (trackowanie aktywności)
- Mobile-friendly PWA
- Admin panel do CRUD pytań i moderacji
- i18n (wielojęzyczność)

To są **dobre kierunki ekspansji**, ale każdy to osobny milestone już po ukończeniu projektu jako portfolio piece.

---

## CHECKLISTA PRZED PIERWSZĄ SESJĄ KODOWANIA

Przed otwarciem Cursora i `dotnet new sln`, upewnij się że:

- [ ] Masz Docker Desktop uruchomiony
- [ ] Node.js 20+ zainstalowany (`node --version`)
- [ ] .NET 9 SDK zainstalowane (`dotnet --version`)
- [ ] PostgreSQL klient (psql lub TablePlus) — przyda się do podglądu bazy
- [ ] Konto GitHuba gotowe, zalogowane w Cursor
- [ ] Folder `F:\Repos\TechQuiz\` istnieje (pusty)
- [ ] Ten plik (Plan roboczy PL) otwarty obok jako reference

---

## NOTATKI I REFLEKSJE

*Tutaj możesz dorzucać własne notatki w trakcie pracy nad projektem — co poszło dobrze, co źle, co zmienić w następnej fazie.*

### 2026-05-11 — Decyzje strategiczne zamknięte
Sesja planistyczna z Claude. 13 ADR-ów spisanych, plan rozbity na 4 fazy + Faza 0. Najtrudniejsza decyzja: nazwa projektu — po sprawdzeniu 11 kandydatów wróciliśmy do roboczej "TechQuiz" bo każda inna nazwa miała kolizje. Wniosek: dla portfolio nazwa nie ma znaczenia, kod ma.

### 2026-05-11 — UI design ukończony
Druga sesja: pełen design system + mockupy. Co zaprojektowane:

- **Design system** — paleta dual theme (slate-950 + violet-500 w dark, white + violet-600 w light), Geist + JetBrains Mono, buttony, inputy, karty, badges, spacing scale
- **5 ekranów × 2 motywy** — Login, Categories, Quiz (Multiple Choice + Code Question Phase 3 forward-look), Result, Dashboard
- **Empty state** dla Dashboard (first-time user view)

Dodatkowe decyzje udokumentowane:
- **ADR-014** — Topbar layout (zamiast sidebar)
- **ADR-015** — Quiz UI patterns (full-screen, keyboard shortcuts, test mode)
- **ADR-016** — Dashboard bento grid (8 tiles, 3-column, varied sizes)

ARCHITECTURE.md rozszerzony o sekcję Component Patterns (code block, explanation block, status pills, metric cards, category icons, progress bars, selected states, empty state hero).

Wnioski z sesji:
- Dual theme to **inwestycja** — wymaga design tokens od początku, ale daje konkretny portfolio talking point
- Quiz w full-screen mode wymaga route-aware shell component (decyzja architektoniczna z designu)
- Empty state musi pokazać disabled placeholdery + main CTA — to "preview" jak dashboard będzie wyglądał
- Pomysł "code zawsze ciemne" nawet w light mode — to świadome złamanie konwencji light/dark, pasuje do dev-toolingu

**Co dalej:** wracamy do kodu/planu Fazy 0. Mockupy są referencją wizualną przy implementacji.
