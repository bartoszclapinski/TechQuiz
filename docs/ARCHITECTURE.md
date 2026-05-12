# Architecture

This document describes the technical architecture of TechQuiz. It is intended for developers reading the code or contributing to the project. For the reasoning behind specific decisions, see [`DECISION_LOG.md`](DECISION_LOG.md).

---

## High-Level Overview

TechQuiz is a full-stack web application with a clear separation between backend (ASP.NET Core API) and frontend (React + TypeScript). The backend follows Clean Architecture with four layers: Domain, Application, Infrastructure, and API. PostgreSQL serves as the primary data store, with Seq providing structured log search during development.

```
┌──────────────────┐         ┌──────────────────┐
│  React Frontend  │ ──────▶ │   ASP.NET Core   │
│   (Vite + TS)    │  HTTPS  │      Web API     │
└──────────────────┘  + JWT  └────────┬─────────┘
                                      │
                      ┌───────────────┼───────────────┐
                      ▼               ▼               ▼
              ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
              │ PostgreSQL  │ │     Seq     │ │  AI Provider│
              │ (EF Core 9) │ │  (Logging)  │ │  (per-user) │
              └─────────────┘ └─────────────┘ └─────────────┘
```

---

## Clean Architecture Layers

The solution uses Clean Architecture with strict dependency rules. Dependencies point inward: Infrastructure and API depend on Application, Application depends on Domain, Domain depends on nothing.

### Domain Layer (`TechQuiz.Domain`)

Pure C# with zero external dependencies. Contains business entities, value objects, enums, and domain interfaces. This is where the core rules live — quiz scoring, attempt validation, spaced repetition logic.

Key entities:
- `Category` — quiz topic grouping (C# Basics, ASP.NET Core, etc.)
- `Question` — a single question with type (multiple choice, code output, code fix, fill-in), difficulty, and answers
- `Answer` — an answer option for a question
- `QuizAttempt` — a user's attempt at a quiz with timing and responses
- `QuizResponse` — individual answer within an attempt
- `UserProgress` — aggregated stats per user per category
- `Badge` — gamification achievement

### Application Layer (`TechQuiz.Application`)

Business logic orchestration. Uses MediatR for CQRS (Command/Query Responsibility Segregation), FluentValidation for input validation, and defines service interfaces that Infrastructure implements.

Key components:
- **Commands** — `StartQuizCommand`, `SubmitAnswerCommand`, `CompleteQuizCommand`
- **Queries** — `GetCategoriesQuery`, `GetQuizResultsQuery`, `GetUserProgressQuery`
- **Services** — `QuizScoringService`, `SpacedRepetitionService` (Phase 2), `QuestionGenerationService` (Phase 3)
- **Validators** — FluentValidation rules for each command

### Infrastructure Layer (`TechQuiz.Infrastructure`)

Everything that touches the outside world: database, external APIs, file system, identity. Implements interfaces defined in Domain and Application.

Key components:
- **Persistence** — `AppDbContext` (EF Core with IdentityDbContext), entity configurations, migrations
- **Identity** — `ApplicationUser` extending `IdentityUser`, JWT token generation
- **AI Providers** (Phase 3) — `OpenAiProvider`, `AnthropicProvider`, all implementing `IAiProvider`
- **Key Vault** (Phase 3) — `EncryptedAiKeyVault` using ASP.NET Core Data Protection
- **Repositories** — implementations of repository interfaces from Domain

### API Layer (`TechQuiz.API`)

ASP.NET Core Web API with controllers, middleware, and Program.cs configuration. Thin layer — controllers receive requests, dispatch via MediatR, return responses.

Key endpoints (MVP):
```
POST   /api/auth/register
POST   /api/auth/login
GET    /api/categories            [Authorize]
POST   /api/quizzes/start         [Authorize]
POST   /api/quizzes/{id}/answer   [Authorize]
POST   /api/quizzes/{id}/complete [Authorize]
GET    /api/quizzes/{id}/result   [Authorize]
```

---

## Authentication Flow

The MVP uses JWT bearer tokens without refresh tokens (deferred to Phase 4). Authentication flow:

1. User registers via `POST /api/auth/register` with email and password
2. ASP.NET Core Identity creates `ApplicationUser` with hashed password (PBKDF2)
3. User logs in via `POST /api/auth/login`
4. Server validates credentials and returns a signed JWT (HS256)
5. Frontend stores token in localStorage and attaches `Authorization: Bearer <token>` to all subsequent requests
6. Middleware validates token on every protected endpoint
7. Token expires after 24 hours (no refresh in MVP — user re-logs in)

Phase 4 will add refresh tokens, email confirmation via SendGrid (or similar), and password reset.

---

## AI Integration Architecture (Phase 3)

The AI layer is the most complex part of the system and is designed for extensibility, cost amortization, and security.

### Multi-Provider Abstraction

```csharp
public interface IAiProvider
{
    Task<GeneratedQuestion> GenerateQuestionAsync(
        QuestionGenerationRequest request,
        string apiKey,
        CancellationToken cancellationToken);

    Task<CodeEvaluation> EvaluateCodeAnswerAsync(
        CodeEvaluationRequest request,
        string apiKey,
        CancellationToken cancellationToken);
}
```

Implementations: `OpenAiProvider`, `AnthropicProvider`. New providers can be added by implementing the interface and registering in DI.

### Per-User API Keys

Each user supplies their own API key for their chosen provider. Keys are encrypted at rest using ASP.NET Core's `IDataProtectionProvider` and stored in the database. Decryption happens only at request time, in memory, and the decrypted key is never logged.

```
User submits API key (HTTPS)
        ↓
Server encrypts via IDataProtectionProvider
        ↓
Encrypted blob stored in `UserAiKeys` table
        ↓
On generation request: decrypt → use → discard
```

### Public Question Pool

When a user generates a question with their API key, the question is saved to the shared question pool with metadata:
- `GeneratedByUserId` — who triggered generation (audit trail)
- `Provider` — which AI provider was used
- `IsAIGenerated = true` — flag for filtering
- `ApprovalStatus` — pending / approved / flagged

Other users can immediately use the generated question without paying for re-generation. This amortizes AI costs across the community.

### Quality Control

AI-generated content requires moderation. Two layers:

**Automatic validation** — schema check (exactly one correct answer, non-empty fields, valid question type, no markdown injection).

**Community voting** — users can upvote/downvote questions. Questions with negative score below a threshold are auto-hidden from the active pool.

### Rate Limiting

Generation endpoints are rate-limited per user (e.g., 20 questions per hour) to prevent accidental cost blowouts. Limits are configured in `appsettings.json`.

---

## Database Schema (MVP)

```
Categories
├── Id (Guid, PK)
├── Name (string)
├── Icon (string)
└── CreatedAt (datetime)

Questions
├── Id (Guid, PK)
├── CategoryId (Guid, FK → Categories)
├── Type (enum: MultipleChoice, CodeOutput, CodeFix, FillIn)
├── Difficulty (enum: Easy, Medium, Hard)
├── Content (string)
├── CodeSnippet (string, nullable)
├── Explanation (string)
├── IsAIGenerated (bool, default false)
├── CreatedAt (datetime)
└── [Phase 3] Provider, GeneratedByUserId, ApprovalStatus

Answers
├── Id (Guid, PK)
├── QuestionId (Guid, FK → Questions)
├── Content (string)
├── IsCorrect (bool)
└── Order (int)

QuizAttempts
├── Id (Guid, PK)
├── UserId (string, FK → AspNetUsers)
├── CategoryId (Guid, FK → Categories)
├── StartedAt (datetime)
├── CompletedAt (datetime, nullable)
├── Score (int, nullable, 0-100)
├── TotalQuestions (int)
├── CorrectAnswers (int)
└── TimeTaken (interval, nullable)

QuizResponses
├── Id (Guid, PK)
├── AttemptId (Guid, FK → QuizAttempts)
├── QuestionId (Guid, FK → Questions)
├── SelectedAnswerId (Guid, FK → Answers, nullable)
├── CodeResponse (string, nullable)
├── IsCorrect (bool)
└── TimeTaken (interval)

UserProgress  [Phase 2]
├── Id (Guid, PK)
├── UserId (string, FK → AspNetUsers)
├── CategoryId (Guid, FK → Categories)
├── TotalAttempts (int)
├── AverageScore (decimal)
├── BestScore (int)
├── CurrentStreak (int)
├── LongestStreak (int)
├── XP (int)
├── Level (int)
└── LastAttemptAt (datetime)
```

Plus standard ASP.NET Core Identity tables: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetRoleClaims`, `AspNetUserTokens`.

---

## Testing Strategy

The project follows **TDD for the Domain and Application layers**. Infrastructure and API layers receive tests after implementation (or via integration tests).

### Domain Tests (`TechQuiz.Domain.Tests`)

Pure unit tests for entities, value objects, and domain services. No mocks needed — Domain has no dependencies.

Example:
```csharp
public class QuizScoringServiceTests
{
    [Fact]
    public void Score_AllAnswersCorrect_Returns100Percent()
    {
        var attempt = QuizAttemptBuilder.WithAllCorrectAnswers(5);
        var score = new QuizScoringService().Calculate(attempt);
        score.Percentage.Should().Be(100);
    }
}
```

### Application Tests (`TechQuiz.Application.Tests`)

Unit tests for CQRS handlers. Dependencies mocked with NSubstitute.

```csharp
public class StartQuizCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsQuizWithQuestions()
    {
        var repo = Substitute.For<IQuestionRepository>();
        repo.GetRandomByCategoryAsync(Arg.Any<Guid>(), 10)
            .Returns(QuestionBuilder.SampleSet(10));

        var handler = new StartQuizCommandHandler(repo);
        var result = await handler.Handle(new StartQuizCommand(categoryId), CancellationToken.None);

        result.Questions.Should().HaveCount(10);
    }
}
```

### Integration Tests (Phase 4)

End-to-end API tests using `WebApplicationFactory<Program>` with a test PostgreSQL container (Testcontainers). Verifies full request → database → response flow.

### Frontend Tests (Phase 4)

Vitest + React Testing Library for component tests. Playwright for end-to-end scenarios (optional).

---

## Logging and Observability

Structured logging via Serilog. All logs include:
- Timestamp (UTC)
- Log level
- Message template with named parameters
- User ID (when authenticated)
- Request ID (correlation across logs)

Two sinks configured:
- **Console** — readable output during development
- **Seq** — searchable structured log server at `http://localhost:5341`

Example:
```csharp
_logger.LogInformation("User {UserId} started quiz in category {CategoryName}",
    userId, category.Name);
```

In Seq, this becomes a queryable event with `UserId` and `CategoryName` as filterable properties.

---

## Frontend Architecture

The React frontend follows a feature-based folder structure:

```
client/src/
├── api/                  # axios instance, JWT interceptor, endpoint functions
├── auth/                 # login, register, useAuth hook, RequireAuth
├── categories/           # category list, category card
├── quiz/                 # quiz flow components, question card, progress bar
├── results/              # results page, answer breakdown
├── dashboard/            # [Phase 2] bento grid dashboard
├── shared/               # reusable UI components
├── lib/                  # utilities, constants
├── App.tsx
└── main.tsx
```

State management:
- **Server state** — TanStack Query (caching, refetching, optimistic updates)
- **Auth state** — React Context (lightweight, only one consumer pattern)
- **Local UI state** — `useState` and `useReducer`

Routing: React Router v6 with protected routes via `<RequireAuth>` wrapper.

### Application Shell

Authenticated users see a topbar shell wrapping all primary screens:

- **Logo** (left) — TechQuiz mark, links to Categories
- **Primary nav** (center) — Categories, Daily Review (soon), Generate (soon), Dashboard (soon), History (soon)
- **User controls** (right) — Theme toggle button, user avatar

The shell is implemented as a route-aware layout component. Routes inside `/quiz/:id` render full-screen (shell hidden) to provide a distraction-free quiz experience. All other authenticated routes render inside the shell.

Phase 2 and 3 nav items are visible but disabled with `soon` badges until their features ship. This communicates the product roadmap directly in the UI.

---

## Visual Design System

The UI follows a **premium SaaS aesthetic** with **dual theme support** (dark default, light alternative) and a violet accent.

### Semantic Design Tokens

All UI components consume CSS variables, never hardcoded colors. Tokens are defined once in `:root` (dark) and overridden in `.light` selector. This enables theme switching by toggling a single class on `<html>`.

| Token | Dark value | Light value | Usage |
|-------|-----------|-------------|-------|
| `--bg-base` | `#020617` (slate-950) | `#ffffff` | Page background |
| `--bg-surface` | `#0f172a` (slate-900) | `#f8fafc` (slate-50) | Cards, panels |
| `--bg-elevated` | `#1e293b` (slate-800) | `#f1f5f9` (slate-100) | Modals, dropdowns |
| `--border-default` | `#1e293b` (slate-800) | `#e2e8f0` (slate-200) | Borders |
| `--border-strong` | `#334155` (slate-700) | `#cbd5e1` (slate-300) | Hover borders, dividers |
| `--text-primary` | `#f8fafc` (slate-50) | `#0f172a` (slate-900) | Primary text |
| `--text-secondary` | `#94a3b8` (slate-400) | `#64748b` (slate-500) | Body text |
| `--text-muted` | `#64748b` (slate-500) | `#94a3b8` (slate-400) | Hints, captions |
| `--accent` | `#8b5cf6` (violet-500) | `#7c3aed` (violet-600) | CTAs, links, highlights |
| `--accent-bg` | `rgba(139,92,246,0.15)` | `#f5f3ff` (violet-50) | Accent surface fills |
| `--accent-text` | `#c4b5fd` (violet-300) | `#6d28d9` (violet-700) | Text on accent backgrounds |
| `--success` | `#10b981` (emerald-500) | `#059669` (emerald-600) | Success states |
| `--warning` | `#f59e0b` (amber-500) | `#d97706` (amber-600) | Warning states |
| `--danger` | `#ef4444` (red-500) | `#dc2626` (red-600) | Error states |

### Theme Toggle

A theme toggle button in the header switches between dark and light modes. Implementation:

1. JavaScript reads `localStorage.theme` (or `prefers-color-scheme` as fallback) on page load
2. Adds `.light` class to `<html>` if light mode is selected
3. Toggle button updates class and persists choice to localStorage
4. CSS variables redefine all colors via the `.light` selector
5. All components automatically rerender with new values (no JS re-render needed)

This is implemented with a placeholder UI in Phase 1 (basic icon button) and polished in Phase 4 (smooth icon transition, system preference detection, no flash on initial load).

### Typography

- **Sans-serif (UI)**: Geist — loaded from Google Fonts, weights 400 and 600
- **Monospace (code)**: JetBrains Mono — loaded from Google Fonts, weight 400
- **Scale**: Tailwind defaults (text-xs through text-4xl)
- **Letter spacing**: `-0.02em` on display sizes (text-2xl+) for tighter, more polished look

### Code Syntax Highlighting

Monaco Editor (Phase 3) and inline code blocks use two themes:
- **Dark mode**: VS Code Dark+ (or One Dark Pro)
- **Light mode**: GitHub Light

The active theme is determined by the same `--theme` value as the rest of the UI, ensuring consistency.

### Layout Patterns

- **MVP screens** — clean centered layouts with generous whitespace
- **Phase 2 dashboard** — bento grid with varied tile sizes for stats, charts, recent activity, achievements
- **Quiz mode** — full-screen, sidebar hidden, focused experience

### Motion

- Hover/focus transitions: `transition-all duration-200`
- Theme transitions: `transition-colors duration-150` on color-bearing elements
- Progress bar updates: `transition: width 0.3s` for smooth quiz progression
- Page transitions: subtle fade-in
- Loading states: shimmer effect on skeleton screens
- No heavy animations — restraint over flash

### Component Patterns

The following recurring patterns are used across multiple screens. Documenting them here so future implementation stays consistent.

**Code block.** Border-left 2px violet, dark background even in light mode (slate-950 in dark theme, indigo-950 in light theme — code stays dark because syntax highlighting reads better on dark backgrounds). Small header bar with language label in monospace + optional Copy button. Used in Quiz code questions and Result explanations.

**Explanation block.** Background `--accent-bg` (violet tinted), border-left 2px violet, monospace uppercase label "Explanation" in muted text, body in slate-300/700. Used in Result wrong-answer breakdowns to highlight pedagogical content.

**Status pills.** 12px monospace text on 10-15% opacity background of semantic color. Used for difficulty badges, correct/wrong indicators, "AI-generated" tags, score trend deltas. Always pill-shaped (border-radius: 999px), always sentence case unless explicitly uppercase metadata.

**Metric cards.** Small label in monospace uppercase (10-11px) above a large value (20-28px, weight 700, letter-spacing -0.02 to -0.04em). Optional trend indicator inline with the value. Used in Result stats grid and Dashboard mini-tiles.

**Category icon tile.** 32-44px rounded square with violet-tinted background and monospace text inside (`C#`, `ASP`, `EF`, `SQL`, etc.). Size varies by context: 26px in lists, 32px in cards, 38px in hero contexts, 44px in tile headers. Consistent across all screens.

**Score progress bar.** 3-6px height (varies by context), full-width within container, slate-800/200 track, violet-500/600 fill. Optional percentage label to the right of the bar in monospace. Used in Categories cards, Login floating tiles, Dashboard streak sparkline.

**Selected state on options.** Three simultaneous visual changes when an option is selected: (1) border switches from default to accent color, (2) any internal indicator (number prefix, checkmark) fills with accent color, (3) outer 3px ring at 15% accent opacity appears. Used in Quiz answer options and any future selectable card pattern.

**Empty state hero card.** Tall centered card with icon in a violet-tinted 48x48 rounded square, "No data yet" heading, descriptive paragraph explaining what will appear, primary CTA button to take the action. Surrounded by disabled (40% opacity) tiles showing the eventual layout filled with em-dash placeholders.

---

## Deployment (Phase 4)

Target platforms under consideration:
- **Railway** — simplest .NET + PostgreSQL hosting with free tier
- **Render** — similar to Railway, generous free tier
- **Azure App Service + Azure Database for PostgreSQL** — most enterprise-relevant for portfolio

Final decision will be made before Phase 4 implementation.

CI/CD pipeline (GitHub Actions):
1. On every PR: build + run all tests
2. On merge to `main`: build + deploy to staging
3. Manual approval: deploy to production

---

## Open Questions and Future Work

- **Refresh tokens** — Phase 4 addition for better UX (no forced re-login every 24h)
- **Email service** — SendGrid for confirmation and password reset (Phase 4)
- **Admin panel** — for managing categories and moderating AI-generated questions (Phase 4+)
- **PWA support** — mobile-friendly with offline capability (Phase 4+)
- **i18n** — currently English-only; future addition for Polish would require resource files

---

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR documentation](https://github.com/jbogard/MediatR)
- [Serilog structured logging](https://serilog.net/)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction)
- [SM-2 spaced repetition algorithm](https://www.supermemo.com/en/blog/application-of-a-computer-to-improve-the-results-obtained-in-working-with-the-supermemo-method)
