# Sprint 1 — Session Log

> Chronological record of work done in Sprint 1 (Phase 1 — MVP).
> Each entry: date, what was done, decisions made, verification result.
> Iteration plans live in `.ai/sprints/sprint1/X.Y-*.md`.

---

## 2026-05-13 — Iteration 1.1 setup: test packages + branch

**Co zrobione:**
- Branched `feat/domain-tdd` off master (`b6eb31c`). One PR will close iteration 1.1.
- Added test packages to `tests/TechQuiz.Domain.Tests`:
  - **FluentAssertions** pinned to `7.*` — last fully OSS (Apache 2.0) version. v8 (January 2025) switched to the Xceed commercial license; v7 stays free and the API surface we use is identical.
  - **coverlet.collector** (latest, currently 10.0.0) — coverage report generator. Plan task 11 requires ≥90% Domain coverage via Coverlet.
- Created this `LOG.md` (Sprint 1 session diary).

**Decyzje:**
- **FluentAssertions v7, not v8.** Avoids the commercial-license trap. If FA v7 ever stops compiling against newer xunit, we'll swap to `Shouldly` (MIT, similar API) or `AwesomeAssertions` (MIT fork of FA v7). Documented now so future-me doesn't reflexively bump.
- **NSubstitute deferred.** Domain layer has no dependencies — mocks aren't needed. NSubstitute lands when iteration 1.2 starts and handler tests need mocked repositories.
- **Strict TDD discipline.** Each invariant follows write-failing-test → make-it-pass → refactor. Commits bundle the test + impl (CI stays green per commit) — the TDD cycle is practiced locally rather than committed as separate red→green commits, to avoid burning CI minutes on intentionally-red intermediate commits.

**Weryfikacja:**
- `cat tests/TechQuiz.Domain.Tests/TechQuiz.Domain.Tests.csproj` → both packages listed:
  ```
  <PackageReference Include="coverlet.collector" Version="10.0.0">
  <PackageReference Include="FluentAssertions" Version="7.*" />
  ```
- `dotnet build TechQuiz.sln` → success, 0 warnings, 0 errors.

---

## 2026-05-13 — Iteration 1.1 complete: Domain layer fully TDD'd

**Co zrobione (7 atomic commits within `feat/domain-tdd` after the setup commit):**

1. `feat(domain): add Category, Question, Option entity skeletons + enums`
   - Entities (immutable, `Guid` Ids): `Category` (Id/Name/Description/IconCode), `Question` (Id/CategoryId/Type/Difficulty/Text/Explanation/Options), `Option` (Id/QuestionId/Text/IsCorrect/OrderIndex).
   - Enums: `Difficulty` (Easy/Medium/Hard), `QuestionType` (MultipleChoice + 3 Phase 3 placeholders).
   - Public constructors initially — invariants added in next commit per TDD discipline.

2. `feat(domain): add Question.Create factory with invariants + tests`
   - TDD cycle: wrote `QuestionTests` first (calling non-existent `Question.Create`) → red (compile fail) → refactored Question to private ctor + static factory with validation → green.
   - Invariants: text non-empty, ≥2 options, MultipleChoice → exactly one correct option.
   - 8 tests including `[Theory]` for boundary cases (0/1 options, "" / " " / "   " text).

3. `feat(domain): add Quiz aggregate with min-1-question invariant + tests`
   - Same private-ctor + factory pattern. Invariant: questions list non-null + ≥1 element.
   - Test verifying `Quiz.Questions` preserves input order (`ImmutableQuestionsOrder` requirement).

4. `feat(domain): add QuizAttempt + Answer with lifecycle (Submit/Complete) + tests`
   - `Answer` = immutable VO (QuestionId, nullable SelectedOptionId, SubmittedAt).
   - `QuizAttempt` = mutable aggregate with `Start` factory + `SubmitAnswer` / `Complete` methods.
   - `SubmitAnswer` replaces existing answer for same question; throws if attempt completed.
   - `Complete` sets CompletedAt; throws if called twice.
   - Clock injected as parameter (testable; no `DateTimeOffset.UtcNow` inside domain).

5. `feat(domain): add Score value object with percentage + difficulty breakdown`
   - `Score.Calculate(questions, answers)` static factory.
   - `Percentage` derived (0% for empty quiz — no div-by-zero).
   - `ByDifficulty` returns `IReadOnlyDictionary<Difficulty, (int Correct, int Total)>`.
   - 9 tests: all-correct, all-wrong, partial 7/10, unanswered=wrong, null-option=wrong, empty quiz, breakdown across Easy/Medium/Hard mix, null-args guards.

6. `feat(domain): add DomainException hierarchy + refactor throws`
   - `DomainException : Exception` (abstract) base.
   - `InvalidQuestionException`, `QuizAlreadyCompletedException` (sealed).
   - Refactored `Question.Create` from `ArgumentException` → `InvalidQuestionException`.
   - Refactored `QuizAttempt.SubmitAnswer/Complete` from `InvalidOperationException` → `QuizAlreadyCompletedException`.
   - Tests updated to assert specific domain exception types.

7. `test(domain): cover Category/Option/Answer ctors + null-options branch (98.8% coverage)`
   - `Category`, `Option`, `Answer` weren't getting their property getters touched by other tests → 76%–88% line coverage. Added focused ctor/property tests for each.
   - Added `Create_WithNullOptions_Throws` test to cover the `options is null` branch of Question.Create.

**Decyzje:**
- **`InvalidQuestionException` (not derived from `ArgumentException`).** Domain-rule violations are conceptually distinct from .NET argument-contract violations — callers can catch all business failures via `catch (DomainException)`. Per iteration plan task 10.
- **`Score` as plain class (not `record`).** Records gain little when most fields are computed and dictionary equality wouldn't be value-based anyway. Plain class with `Calculate` factory is clearer.
- **`Answer.SelectedOptionId` nullable** to model "left unanswered". Scoring counts these as wrong without an explicit "unanswered" enum value.
- **Clock injected as parameter (`DateTimeOffset submittedAt`/`completedAt`).** No `IClock` abstraction yet (over-engineering for Phase 0) — Application layer will use whatever it wants and pass values in. Domain stays pure.
- **`Score.Calculate` accepts `IEnumerable<>` (not `IReadOnlyList<>`).** Caller flexibility; we enumerate once internally.
- **Coverage at 98.8% line / 100% branch.** Well above the DoD ≥90% threshold. Question stays at 95% — the two uncovered lines are the closing braces of throw blocks (Coverlet quirk), not real gaps.

**Weryfikacja (Definition of Done):**
- [x] Entities defined: Category, Question, Option, Quiz, QuizAttempt, Answer.
- [x] `Difficulty` enum + `QuestionType` enum.
- [x] `Score` value object with percentage + ByDifficulty.
- [x] Domain exceptions (`DomainException` base + 2 sealed types).
- [x] `dotnet test TechQuiz.Domain.Tests` → 37 tests, all green.
- [x] Coverage ≥90% (actual: line 98.8%, branch 100%) via Coverlet `XPlat Code Coverage` collector.

**Test breakdown:**
- AnswerTests: 2
- CategoryTests: 1
- DomainExceptionTests: 3
- OptionTests: 2
- QuestionTests: 9
- QuizAttemptTests: 7
- QuizTests: 4
- ScoreTests: 9
- **Total: 37**

**Pauza — punkt wznowienia:**
- Branch: `feat/domain-tdd` — 8 commits stacked on master, ready to push + PR.
- Next: open PR closing iteration 1.1, then iteration 1.2 (Application layer with MediatR + FluentValidation + handler tests).

---

## 2026-05-13 — Iteration 1.2 session 1: scaffolding + first handler

**Co zrobione (6 commits on `feat/application-layer`, not yet pushed):**

1. `chore(application): add MediatR + FluentValidation + test packages`
   - Application: `MediatR 14.1.0` (Apache 2.0 — current published versions remain OSS, Jimmy Bogard announced future commercial split but no concrete cutoff yet), `FluentValidation 12.1.1`, `FluentValidation.DependencyInjectionExtensions 12.1.1`, `Microsoft.Extensions.Logging.Abstractions 10.0.8` (forced up by MediatR 14's transitive constraint — runs fine on `net9.0`).
   - Application.Tests: `FluentAssertions 7.*` (pinned per FA v8 licensing change), `NSubstitute 5.x`, `coverlet.collector`.

2. `feat(application): add abstractions (UserContext, UnitOfWork, repositories)`
   - `Abstractions/IUserContext.cs` — `Guid UserId { get; }`. JWT-claim wiring lives in Infrastructure (iter 1.3).
   - `Abstractions/IUnitOfWork.cs` — `SaveChangesAsync(CT)`.
   - `Abstractions/ICategoryRepository.cs` — `GetAllAsync`, `GetQuestionCountsAsync` (batch, avoids N+1), `GetUserBestScoresAsync(userId)`.
   - `Abstractions/IQuizRepository.cs` — `GetByCategoryAsync`, `GetAttemptAsync`, `AddAttemptAsync`, `GetAttemptsByUserAsync`.

3. `feat(application): add shared DTOs (Category, Question, Option, Answer)`
   - All as `sealed record` in `Common/Dtos/`.
   - `OptionDto` deliberately omits `IsCorrect` (CLAUDE.md Hard Rule #4).
   - `QuestionDto` omits `Explanation` too — reveal logic stays out of in-quiz payload. A separate `QuestionResultDto` with `IsCorrect` + `Explanation` will land alongside `CompleteQuizCommand` in a later session.
   - `AnswerDto` allows `SelectedOptionId == null` (unanswered).

4. `feat(application): add Validation + Logging pipeline behaviors`
   - `Common/Behaviors/ValidationBehavior<TRequest, TResponse>` runs every `IValidator<TRequest>` before the handler; aggregates failures into a single `FluentValidation.ValidationException`.
   - `Common/Behaviors/LoggingBehavior<TRequest, TResponse>` logs request type name + duration + success/failure via `ILogger`. No payload logging — payloads can contain PII (e.g. `RegisterCommand` password).

5. `feat(application): wire MediatR + FluentValidation + behaviors via AddApplication`
   - `DependencyInjection.AddApplication(IServiceCollection)` does the trio: `RegisterServicesFromAssembly` for MediatR, `AddOpenBehavior` for Logging then Validation (outer-to-inner order), `AddValidatorsFromAssembly` for FluentValidation.

6. `feat(application): add GetCategoriesQuery handler + tests (TDD)`
   - `Features/Categories/GetCategoriesQuery.cs` — empty record (no params, returns `IReadOnlyList<CategoryDto>`).
   - `Features/Categories/GetCategoriesQueryHandler.cs` — three repo round-trips (categories, question counts, user best scores) projected into `CategoryDto` list.
   - 4 NSubstitute-mocked tests: happy path with 2 categories + score, empty case, category-without-questions edge, scoping of best-scores call to current user id.

**Decyzje:**
- **MediatR 14 (not pinned to a "free" version).** Currently published versions are Apache 2.0. If future MediatR versions go commercial we can either pin to the last free version or migrate to `Mediator` (martinothamar) — they share concepts. Defensive pinning now would be premature.
- **`Microsoft.Extensions.Logging.Abstractions 10.x` on `net9.0`.** Transitively required by MediatR 14. Compatible with .NET 9 (multi-target). Different from the EF Core 10 → net9 incompatibility we hit earlier — for that one, the 10.x packages were truly net10-only.
- **Vertical slice folders.** `Features/Categories/GetCategoriesQuery.cs` + same-folder handler. Each feature lives in one place, easy to find. When a feature grows additional artifacts (validators, sub-DTOs), they cohabit. Wider Common/ folder for truly shared concerns (`Abstractions/`, `Behaviors/`, `Dtos/`).
- **`OptionDto` and `QuestionDto` omit reveal fields** even though the field is `private` in the Domain entity — defense in depth at the boundary. Two DTO types (in-quiz vs post-complete) is intentional duplication for safety.
- **Pipeline order: Logging outer, Validation inner.** So validation failures get logged with their request name. Reversed order would silently drop the log if validation throws.
- **Batched `GetQuestionCountsAsync` instead of `CountQuestionsAsync(categoryId)`.** Refactored the interface during the handler-writing phase — the per-category variant would have triggered N+1 in the handler's iteration. Single dictionary return makes the handler obvious and the repo implementation efficient.
- **No FluentValidation validator yet for `GetCategoriesQuery`.** Empty record, nothing to validate. Behavior gracefully skips when no validators registered.
- **Handler tests bypass MediatR.** Direct instantiation of the handler class with mocked deps + `await handler.Handle(...)`. Testing through `IMediator.Send` would couple unit tests to pipeline behavior — that's an integration concern.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → success, 0 warnings, 0 errors.
- `dotnet test tests/TechQuiz.Application.Tests` → 4 passed, 0 failed.
- Domain layer tests still green (37 from iteration 1.1).

**Punkt wznowienia:**
- Branch: `feat/application-layer` — 6 commits stacked on master, not pushed yet (one PR per iteration policy — push when iteration 1.2 is done).
- Next session: `StartQuizCommand` + `SubmitAnswerCommand` handlers + FluentValidation validators + tests. New abstractions on `IQuizRepository` are already in place.
- Remaining iteration 1.2 work after that: `CompleteQuizCommand`, `GetAttemptHistoryQuery`, coverage verification, PR.

---

## 2026-05-13 — Iteration 1.2 session 2: remaining handlers + close

**Co zrobione (5 commits on `feat/application-layer`):**

1. `feat(application): add StartQuizCommand + handler + validator + tests`
   - `Features/Quizzes/StartQuizCommand.cs` (record: `CategoryId`).
   - `Features/Quizzes/QuizSessionDto.cs` (narrow, feature-folder: `AttemptId` + `IReadOnlyList<QuestionDto>`).
   - Handler: `IQuizRepository.GetByCategoryAsync` → `QuizAttempt.Start` (via `TimeProvider`) → `AddAttemptAsync` → `SaveChangesAsync`. Returns projection without `IsCorrect` on options.
   - Validator: `CategoryId.NotEmpty`.
   - 6 tests: happy path, persistence/save assertion, current-user assignment, category-without-quiz throws `KeyNotFoundException` + side effects suppressed. + 2 validator tests.

2. `feat(application): add SubmitAnswerCommand + handler + validator + tests`
   - Added `IQuizRepository.GetByIdAsync(quizId)` — needed to validate "question belongs to attempt's quiz".
   - Handler does 4 guard checks: attempt exists → belongs to user → not completed → question belongs to quiz. Each fails with a specific exception. Then `attempt.SubmitAnswer(...)` and save.
   - 7 handler tests covering all guard paths + happy path (with-null-option = unanswered) + replacement of earlier answer. + 4 validator tests.

3. `feat(application): add CompleteQuizCommand + handler + tests (QuizResultDto)`
   - Added 3 shared DTOs for the result view: `OptionResultDto` (with `IsCorrect`), `QuestionResultDto` (with `Explanation`, `UserSelectedOptionId`, `IsCorrect`), `DifficultyBreakdownDto`.
   - `Features/Quizzes/QuizResultDto.cs` — composite DTO returned by the command (score + per-question breakdown).
   - Handler: same 3 guards as `SubmitAnswerCommand` minus question-belongs check. Then `attempt.Complete(now)`, save, `Score.Calculate(quiz.Questions, attempt.Answers)`, project full result DTO.
   - 6 handler tests: all-correct/100%, partial mix (correct/wrong/unanswered), options expose `IsCorrect` post-complete, all 3 guard paths. + 2 validator tests.

4. `feat(application): add GetAttemptHistoryQuery + handler + validator + tests`
   - `Common/Dtos/AttemptHistoryItemDto.cs` (shared — Phase 2 dashboard will reuse).
   - `Features/Quizzes/GetAttemptHistoryQuery.cs` with default `Page=1, PageSize=20`.
   - Handler does `(page - 1) * pageSize` to skip, calls `IQuizRepository.GetAttemptsByUserAsync(userId, skip, take, ct)`, projects to lightweight DTO (no score data — that's loaded on-demand for attempt detail view).
   - 4 handler tests: projection with completion flag, pagination math, user-scoping, empty case. + 4 validator tests (page < 1, page-size out of [1, 100] bounds, defaults pass, max-size 100 passes).

5. `test(application): add tests for Validation + Logging pipeline behaviors`
   - `Common/Behaviors/ValidationBehaviorTests` — no-validators pass-through, valid request passes, failing validator throws `ValidationException` and suppresses handler call, aggregated failures from multiple validators.
   - `Common/Behaviors/LoggingBehaviorTests` — successful request logs Information level; throwing handler logs Error with the exception, then rethrows.
   - NSubstitute gotcha: nested `TestRequest` record had to be `public` (not `private`) because FluentValidation/MS.E.Logging are strong-named — Castle DynamicProxy can't proxy `IValidator<TInternalRequest>` across assembly boundaries unless the request type is publicly accessible.

**Decyzje:**
- **`TimeProvider` injected** to handlers that timestamp domain operations. Built-in `System.TimeProvider` (added .NET 8) — no extra package, NSubstitute mocks it cleanly. Default `TimeProvider.System` will be wired by Infrastructure DI in iter 1.3 (or by Api `Program.cs`).
- **`KeyNotFoundException`** for "attempt/quiz not found" cases. Generic .NET exception that API layer can map to 404 generically. If we ever need more specific types (e.g., to distinguish "quiz vs attempt"), wrap them later — no need today.
- **`UnauthorizedAccessException`** for "attempt belongs to another user". System namespace, maps cleanly to 403 in API layer.
- **`QuizAlreadyCompletedException` reused** from the Domain layer (introduced iter 1.1). It's thrown both by Domain on guard violation AND by handlers as a fail-fast check before calling domain methods. Single exception type for one logical condition.
- **`ArgumentException` for "question not in quiz"** in SubmitAnswerCommand. It's not a domain rule violation (Domain doesn't enforce question-belongs-to-quiz — that's a coordination concern between aggregates). Argument-level rejection is the right level.
- **`InvalidOperationException` for "attempt references missing quiz"** in handlers — this is a data-corruption signal (attempts pointing at deleted quizzes shouldn't happen). Distinct from KeyNotFoundException (which is "you asked for X and we don't have one").
- **No score data on `AttemptHistoryItemDto`.** History page only shows when/what — score computed on detail click. Future Phase 2 dashboard with rich stats can use a separate `AttemptStatsDto` aggregating across attempts.
- **`GetAttemptHistoryQuery.Page` and `PageSize` as record positional params with defaults.** Matches REST convention `?page=1&pageSize=20` mapping cleanly via ASP.NET binding.
- **Tests for DependencyInjection.AddApplication skipped.** Integration-style "wire it up, dispatch a request, verify pipeline behavior fires" tests belong in iteration 1.4 (API smoke tests via `WebApplicationFactory`).
- **`KeyNotFoundException` is fine for handler tests** even though we're using FluentAssertions — `ThrowAsync<KeyNotFoundException>` works the same as for other types.

**Weryfikacja (DoD):**
- `dotnet build TechQuiz.sln` → 0 warnings, 0 errors.
- `dotnet test tests/TechQuiz.Application.Tests` → **46/46 passed**.
- Combined `dotnet test TechQuiz.sln` → **83 tests** (37 Domain + 46 Application).
- Application csproj contains no `Microsoft.EntityFrameworkCore` or `Npgsql` reference.
- Per-class line coverage of Application: handlers + validators at **100%**, pipeline behaviors at **100%** (after smoke tests added), DTOs partial (record auto-gen members not exercised — not real gaps), `DependencyInjection.cs` 0% (DI registration is integration territory).

**Branch status:**
- `feat/application-layer` — 12 commits stacked on master, pushed PR coming next.
- Iteration 1.2 closed. Ready for push + PR to master.

---

## 2026-05-15 — Iteration 1.3 session A: EF Core mappings + initial migration

**Co zrobione (single PR `feat/iteration-1.3-persistence-A`, merged as #17):**

1. **`AppDbContext`** inherits `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` — keyed by `Guid` to match Domain entity IDs. Exposes `DbSet<Category>`, `DbSet<Question>`, `DbSet<Option>`, `DbSet<Quiz>`, `DbSet<QuizAttempt>`, `DbSet<Answer>`.
2. **`ApplicationUser : IdentityUser<Guid>`** in `Persistence/Identity/`. Empty for now — extension points (display name, avatar URL) deferred until Phase 2 dashboard work needs them.
3. **`IEntityTypeConfiguration<T>` per entity** in `Persistence/Configurations/`: `CategoryConfiguration`, `QuestionConfiguration`, `OptionConfiguration`, `QuizConfiguration`, `QuizAttemptConfiguration`. Keys, indexes (`CategoryId` on Question, `UserId` on QuizAttempt), foreign keys with cascade behaviors, and constructor-binding for private-ctor aggregates. `Answer` is owned by `QuizAttempt` (no separate config file).
4. **`UseTechQuizConventions()` extension method** wraps `EFCore.NamingConventions.UseSnakeCaseNamingConvention()`. Single point if we ever swap the convention package or need to layer additional conventions. ASP.NET Identity tables intentionally keep their PascalCase names (`AspNetUsers` etc.) — the convention applies only to domain tables via per-table overrides.
5. **`DesignTimeDbContextFactory`** lets `dotnet ef` commands construct `AppDbContext` without booting the full API. Connection string resolution: env var `DOTNET_EF_CONNECTION_STRING` → fallback to docker-compose dev default (`Host=localhost;Port=5433;...`).
6. **Initial migration `20260515145322_InitialCreate`** generated, inspected (no surprises in generated SQL), and applied cleanly to fresh PostgreSQL on port 5433.

**Decyzje:**
- **`IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`** — `Guid` PKs throughout (matches Domain entity IDs). `int` Identity defaults would force a translation layer everywhere Identity meets domain code.
- **EF can't bind navigation collections through constructor.** First pass had `Question.Create(IReadOnlyList<Option>)` and `Quiz.Create(IReadOnlyList<Question>)` — EF threw at materialization because it can't pump navigations into ctor params. Refactor: private parameterless ctor + `{ get; init; }` scalars + private `List<T>` backing fields for collections + public `IReadOnlyList<T>` projections. Factory `Create` methods stay the public surface; EF uses the private ctor.
- **Snake-case for domain tables, PascalCase for Identity tables.** Identity tables come from the framework and are referenced in many SQL queries documented online with their PascalCase names — keeping the standard avoids confusion. Domain tables follow PostgreSQL convention.
- **Separate `IEntityTypeConfiguration<T>` files, not inline in OnModelCreating.** Easier to find, easier to evolve, no 400-line `OnModelCreating`. The `Answer` exception (owned, so configured inside `QuizAttemptConfiguration`) is the only collocation.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → 0 warnings, 0 errors.
- `dotnet ef migrations script` → SQL inspected, no missing FKs or surprise indexes.
- `dotnet ef database update` against docker-compose postgres → tables created, `__EFMigrationsHistory` shows `20260515145322_InitialCreate` applied.

---

## 2026-05-17 — Iteration 1.3 session B: repositories + UnitOfWork + UserContext + DI

**Co zrobione (single PR `feat/iteration-1.3-persistence-B`, merged as #27):**

1. **`CategoryRepository : ICategoryRepository`** — `GetAllAsync`, `GetQuestionCountsAsync` (batch `GroupBy(c => c.Id).ToDictionaryAsync` to avoid N+1), `GetUserBestScoresAsync(userId)` (joins `QuizAttempt`s and projects best score per category).
2. **`QuizRepository : IQuizRepository`** — `GetByIdAsync`, `GetByCategoryAsync` (with `.Include(q => q.Questions).ThenInclude(q => q.Options)` since the quiz-start flow needs the full graph), `AddAttemptAsync`, `GetAttemptAsync`, `GetAttemptsByUserAsync(userId, skip, take)`.
3. **`HttpUserContext : IUserContext`** — reads `ClaimTypes.NameIdentifier` from `IHttpContextAccessor.HttpContext?.User` and parses as `Guid`. Throws `InvalidOperationException` if no claim present or unparseable — these are programmer errors (handler running without auth middleware) not user errors.
4. **`UnitOfWork : IUnitOfWork`** — thin wrapper over `AppDbContext.SaveChangesAsync(CT)`. The abstraction keeps Application layer free of EF references.
5. **`Infrastructure/DependencyInjection.cs`** — `AddInfrastructure(IServiceCollection, IConfiguration)` registers `AppDbContext` (scoped), the two repositories (scoped), `UnitOfWork` (scoped), `HttpUserContext` as `IUserContext` (scoped). `IHttpContextAccessor` registration stays in API host (HTTP-specific, see ADR-001).

**Decyzje:**
- **Repositories are sealed.** No inheritance from a generic `Repository<T>` base — ADR-004 settled this. Each repository owns its query shapes.
- **`GetQuestionCountsAsync` returns `IReadOnlyDictionary<Guid, int>`.** Caller can `.TryGetValue` for categories with zero questions without an extra query — eliminates the N+1 trap from the original `CountQuestionsAsync(categoryId)` shape.
- **`GetByCategoryAsync` eagerly loads the full quiz graph.** The `StartQuizCommand` flow needs Questions + Options together to project `QuestionDto`s. Lazy loading is intentionally not enabled (would mask N+1s behind innocent property access).
- **`HttpUserContext` throws on missing claim**, doesn't return `Guid?`. Handlers that need the user already assume an authenticated user — making the type non-nullable forces auth-middleware-first composition. Anonymous endpoints don't take `IUserContext`.
- **`AddInfrastructure` doesn't register `IHttpContextAccessor`.** HTTP concern, belongs in the API host. Infrastructure stays composable in non-HTTP test/utility hosts.

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → 0 warnings, 0 errors.
- Application + Domain tests still 83/83 (no regression — Application's interfaces unchanged).
- Manual smoke via `dotnet user-secrets set "Jwt:SigningKey" "<...>"` + `dotnet run --project src/TechQuiz.Api` → `/health` returns 200 with `postgres` check `Healthy`.

---

## 2026-05-18 — Iteration 1.3 session C: seed data + three code-review rounds

**Co zrobione (single PR `feat/iteration-1.3-seed`, merged as #35 after 3 review rounds):**

1. **Initial 4 commits — seeder + question content:**
   - `feat(infra): configure ASP.NET Core Identity services for UserManager runtime` — `AddIdentityCore<ApplicationUser>` (not `AddIdentity` — would conflict with JWT bearer scheme). Adds `.AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>()`.
   - `feat(infra): add DataSeeder skeleton seeding category, quiz and demo user` — Initial pass with single global `AnyAsync` guard.
   - `feat(infra): seed Unit Testing question bank (19 questions from EPAM 003)` — 471-line `UnitTestingQuestions.cs` factory. 19 questions, 14 from `11-quiz` + 5 from `12-test-doubles`. Difficulty mix: 6 Easy / 10 Medium / 3 Hard.
   - `feat(api): wire DataSeeder into Program.cs startup pipeline` — Development-gated, `using var scope` because seeder is scoped (`AppDbContext` + `UserManager` dependencies).

2. **Review round 1 fixes (3 commits):**
   - `fix(infra): make seeder idempotent per-resource` (closes #36) — Replaces single global `AnyAsync(Categories)` guard with per-resource checks: `SeedCategoryIfMissingAsync(name)` keys on category Name, `SeedDemoUserAsync` keys on email via `FindByEmailAsync`. Partial-failure paths heal on next boot.
   - `feat(api): log seed failures critically before rethrowing` (closes #37) — `try/catch` in `Program.cs` around `seeder.SeedAsync()` with `LogCritical` + rethrow. Host-aborts behaviour unchanged; only forensics improved.
   - `docs(infra): note absence of CancellationToken overload on UserManager.CreateAsync` (closes #38) — inline comment so future readers don't waste time looking for an overload that doesn't exist.

3. **Review round 2 fixes (3 commits):**
   - `fix(infra): bind Identity password policy from configuration` (closes #39) — Originally `AddIdentityCore` set `RequiredLength = 8` and `RequireNonAlphanumeric = false` unconditionally. The seeder was dev-gated, but the policy itself was leaking to staging/prod. Refactored to bind `options.Password` from `Identity:Password` config section. `appsettings.json` ships prod-safe defaults (length 12, all character classes required). `docker-compose.yml` initially overrode via env vars — see round 3.
   - `feat(api): widen seed try/catch to cover scope and resolve` (closes #40) — `CreateScope()` and `GetRequiredService<DataSeeder>()` now sit inside the try block. DI registration regressions log the same critical line.
   - `feat(infra): throw on cancellation before UserManager.CreateAsync` (closes #41) — Explicit `cancellationToken.ThrowIfCancellationRequested()` before the lone non-cancellable Identity call. Host-shutdown cooperative even though the call itself can't be cancelled mid-flight.

4. **Review round 3 fixes (2 commits):**
   - `fix(infra): move Identity dev-override to appsettings.Development.json` (closes #42) — Round 2 placed dev relaxations as `Identity__Password__*` env vars in `docker-compose.yml`, but those don't load for `dotnet run` outside Docker → seed throws on prod-safe policy. Moved to `appsettings.Development.json` (un-ignored in `.gitignore` — no secrets, just dev defaults). Single source of truth for both `dotnet run` and `docker compose up`.
   - `feat(infra): validate Identity password options on startup` (closes #43) — `services.AddOptions<IdentityOptions>().Validate(o => o.Password.RequiredLength >= 8, ...).ValidateOnStart()`. A misconfigured deploy slot setting `RequiredLength=0` now fails at boot instead of silently weakening auth.

**Decyzje:**
- **EPAM Unit Testing module first.** Owner's directive — focus on EPAM Fundamentals 003 material before broader category coverage. SQL + EF Core categories get their own seed in later iterations.
- **Question Q02 rephrased to NOT-question.** Source had "Which THREE frameworks are used?" with MSTest/NUnit/xUnit correct. Domain invariant requires exactly one correct option for MultipleChoice. Rephrased to "Which is NOT a framework?" with JUnit as the single correct answer. Documented inline + in commit message + in PR body — triple-redundant for future readers.
- **Per-resource idempotency from round 1.** Single global guard didn't scale past one category and left a partial-failure trap. Pattern that emerged (`SeedCategoryIfMissingAsync(name, …)`) makes adding a second category in iteration 1.4 a one-liner with zero regression risk.
- **Config-driven Identity policy with env-specific overrides.** Avoided the temptation to add `if (env.IsDevelopment())` branches in `Infrastructure/DependencyInjection.cs` — that would couple Infrastructure to environment knowledge. Pure configuration binding lets each env file own its policy.
- **`ValidateOnStart` floor at 8 chars.** Matches NIST minimum and the dev override. If someone misconfigures a slot to `RequiredLength=0`, the host fails at boot rather than at first sign-in (which is what `Validate()` would catch but in a worse place — runtime, not startup).
- **`DemoUserPassword` stays `public const`** for this PR. Owner deferred the `internal const` + `[InternalsVisibleTo]` change to session D where the test project setup lands naturally.

**Weryfikacja (end-to-end smoke, fresh volume + fresh migrations):**

Path A — `dotnet run` outside Docker (the regression path round 3 fixed):
```
[16:53:02 INF] Seeded category Unit Testing with 19 questions, quiz a1ccba28-...
[16:53:02 INF] Seeded demo user demo@techquiz.local
[16:53:03 INF] Application started.
```

Path B — `docker compose up`:
```
[14:58:53 INF] Seeded category Unit Testing with 19 questions, quiz cfccd141-...
[14:58:54 INF] Seeded demo user demo@techquiz.local
[14:58:54 INF] Application started.
```

DB state in both runs:
```
 categories | questions | options |      demo_user
------------+-----------+---------+---------------------
          1 |        19 |      76 | demo@techquiz.local
```

`19 × 4 = 76` options confirms the join wiring. Re-running the API → `Seed skipped` log lines for both resources (per-resource idempotency proven).

**Deferred to session D:**
- `internal const` for `DemoUserPassword` + `[InternalsVisibleTo]` to test assembly.
- End-to-end DB smoke as an integration test (planned: first Testcontainers integration test calls `SeedAsync` twice, asserts `categories=1, questions=19, options=76` after both calls).

---

## 2026-05-18 — Iteration 1.3 follow-up: validate startup options before seeder

**Co zrobione (single-commit PR `chore/seed-startup-validation-ordering`, merged as #45):**

`chore(api): validate startup options before running seeder` (closes #44) — `app.Services.GetRequiredService<IStartupValidator>().Validate()` immediately before the dev seeder block. Closes a timing nuance flagged in the final review of #35: `ValidateOnStart` normally fires inside `IHost.StartAsync` (during `app.Run()`), so without an explicit `Validate()` call earlier, a misconfigured policy would let the seeder create the demo user before validation aborts the host.

**Decyzje:**
- **One-liner over architecturally cleaner alternatives.** Converting the seeder to `IHostedService` would also fix the ordering but pulls in lifecycle handling that the dev-only block doesn't need. `IStartupValidator.Validate()` is one call that says exactly what it does.
- **Scope to `IsDevelopment()` block.** Validation is dev-only because the seeder is dev-only. In staging/prod the `IStartupFilter` registered by `ValidateOnStart` still fires inside `IHost.StartAsync` — full coverage on every path, no gap.

**Weryfikacja (DB-independent because validation runs before any DB query):**
- Misconfig (`Identity__Password__RequiredLength=0`): `OptionsValidationException` thrown at `Program.cs:89` (the new `Validate()` call). `grep -c "Seeded|Data seeding|Seed skipped" misconfig.log` → `0`. Host aborts **before** any seeder log line.
- Good config (DB intentionally unreachable on port 65432): no `OptionsValidationException` (validate passes silently). Seeder runs, fails at DB connect, hits the existing critical-log + rethrow path. Confirms the new call is a no-op for valid config.

---

## 2026-05-18 — Iteration 1.3 session D start: docs catch-up + integration test setup

**Co zrobione (this commit — branch `feat/iteration-1.3-integration-tests`):**

- Caught up `LOG.md` with four iteration 1.3 entries (sessions A/B/C + PR #45) — entries above this one.
- Ticked the delivered DoD items in `1.3-persistence.md` and added a note on the scope deferral for the "2 categories with 15+ questions each" item (one category shipped — second deferred to iteration 1.4).

**Pozostałe commity zaplanowane na sesję D:**
1. `chore(infrastructure-tests): add Testcontainers + FluentAssertions + coverlet` — package dependencies for real-Postgres integration tests.
2. `chore(infra): scope DemoUserPassword to internal with InternalsVisibleTo` — closes the deferred review item #5 from session C.
3. `feat(infrastructure-tests): add PostgresContainerFixture + IntegrationTestBase` — xUnit collection fixture that spins one Postgres container per test class, applies migrations, exposes a fresh `AppDbContext` per test.
4. `test(infrastructure): add CategoryRepository integration tests` — round-trip + the deferred end-to-end DB smoke (`SeedAsync` twice, assert counts).
5. `test(infrastructure): add QuizRepository integration tests` — full quiz graph save/retrieve, cascade-delete behavior, attempts query with pagination.

---

## 2026-05-21 — Iteration 1.3 session D complete + review-feedback refactor

**Co zrobione (8 commits on `feat/iteration-1.3-integration-tests`, merged as #53):**

1. `docs(sprint1): catch up LOG and iteration 1.3 DoD checkboxes` (closes #46) — the catch-up entry above plus DoD updates.
2. `chore(infrastructure-tests): add Testcontainers + FluentAssertions` (closes #47) — `Testcontainers.PostgreSql` and `FluentAssertions` (later pinned to 7.2.2 / 4.11.0).
3. `chore(infra): scope DemoUser constants to internal with InternalsVisibleTo` (closes #48) — closes the deferred review item from #35 round 1. `DemoUserEmail/Name/Password` move to `internal`, `Properties/AssemblyInfo.cs` exposes the assembly's internals to the test project.
4. `feat(infrastructure-tests): add PostgresContainerFixture + IntegrationTestBase` (closes #49) — initial fixture + base class + `DatabaseCollection`.
5. `test(infrastructure): add CategoryRepository integration tests` (closes #50) — 5 tests, one of which (placeholder GetUserBestScoresAsync) was later removed in commit 8.
6. `test(infrastructure): add DataSeeder integration smoke tests` (closes #51) — closes the deferred E2E DB smoke from #35: 1 category / 19 questions / 76 options / 1 demo user, plus idempotent second invocation.
7. `test(infrastructure): add QuizRepository integration tests` (closes #52) — 7 tests covering full graph load, attempt round-trip with owned `Answer`s, paginated user-scoped attempts.
8. `refactor(infrastructure-tests): address review feedback on PR #53` (closes #54) — owner left 7 inline review comments, all non-blocking but each pointing at real drift/quality concerns. Consolidated fix:
   - Deleted the placeholder `GetUserBestScoresAsync` test (testing a placeholder advertises a feature that does not exist).
   - `PostgresContainerFixture` discovers truncate targets from `information_schema.tables` once after migrations — adding a domain or Identity table no longer requires editing the test base.
   - `SeedCategoryWithQuizAsync` returns the full `Question[]` so `GetAttemptAsync_LoadsOwnedAnswers` uses a real `Option.Id`.
   - Dropped a redundant comment.
   - `IntegrationTestBase.CreateUserAsync` helper uses `UserManager.CreateAsync` instead of manual `NormalizedUserName/SecurityStamp` plumbing — drift-resistant.
   - Pinned `FluentAssertions 7.2.2` / `Testcontainers.PostgreSql 4.11.0` (matches the rest of the solution).
   - Moved the shared `IServiceProvider` to `PostgresContainerFixture.Services` (Identity + DataSeeder registered once after migrations). `DataSeederTests.RunSeedAsync` is now a 3-line scope+resolve+invoke. Single source of truth, closer to production DI.

**Decyzje:**
- **Shared SP on the fixture, not the test base.** Per-test SP would add noise (3-4s of Identity initialization × every test) and lose the "single source of truth" benefit. Per-fixture means one container, one migration pass, one DI build — only TRUNCATE pays per-test cost.
- **TRUNCATE driven by `information_schema.tables`.** Initially had a hardcoded list. Reviewer flagged: adding a table = silent drift. Schema-driven means zero test changes for future migrations. `__EFMigrationsHistory` is excluded so the schema survives between tests.
- **EF1002 (raw-SQL injection) suppressed locally** with a comment explaining the source is the schema catalog. Identifiers cannot be parameterised in DDL; rely on the input provenance instead.
- **`internal const string DemoUser*`** + `[assembly: InternalsVisibleTo("TechQuiz.Infrastructure.Tests")]` — credentials aren't secret but `public` mis-signals "library API surface". Visibility relaxation belongs alongside the test project that consumes them.

**Weryfikacja (DoD):**
- `dotnet test TechQuiz.sln` → **96/96 pass** (37 Domain + 46 Application + 13 Infrastructure).
- Container startup ~52s + tests ~30s = ~1m25s total on first run; subsequent runs container-startup-bound.
- All 7 review concerns explicitly addressed in PR #53 comment trace.
- Iteration 1.3 DoD now fully met (one item with documented scope adjustment for "2 categories" → 1 shipped, 1 deferred to 1.4).

**Iteracja 1.3 zamknięta.** Ready for iteration 1.4.

---

## 2026-05-21 — Iteration 1.4 session A start: API auth endpoints

**Cel sesji A:** Najszybsza ścieżka do "dotnet run + Postman zwracają JWT". Po tej sesji można się zalogować jako `demo@techquiz.local` / `Demo123!` przez REST i dostać access + refresh token.

**Co już mamy gotowe** (z iteracji 1.3):
- JWT bearer middleware wpięte w `Program.cs` z `TokenValidationParameters` (issuer/audience/signing-key/clock-skew).
- `ApplicationUser : IdentityUser<Guid>` + `UserManager` w DI z password policy bound from config.
- `IUserContext` (`HttpUserContext`) czyta `NameIdentifier` claim z JWT.
- Identity DB schema (AspNet* tables) z migracji `InitialCreate`.

**Co brakuje** (sesja 1.4-A dostarcza):
- Domain `RefreshToken` aggregate z TDD-zaaplikowanymi inwariantami (issue/revoke/expiry).
- Application: `RegisterCommand`, `LoginQuery`, `RefreshCommand` + validators + handler tests.
- Infrastructure: `RefreshToken` EF mapping + migracja `AddRefreshTokens` + `RefreshTokenRepository`.
- Infrastructure: `JwtTokenService` (wystawia access tokeny z claims, signed via configured key).
- API: `AuthController` z 3 actions (POST register/login/refresh).
- Postman collection (minimum: register + login + refresh w docs/postman/).

**Plan commitów (7 atomic, jeden PR zamykający sesję A):**

1. `docs(sprint1)`: this entry + status flag flips.
2. `feat(domain)`: `RefreshToken` aggregate + tests (TDD — invariants for issue/revoke/expiry).
3. `feat(application)`: auth commands + validators + handler tests (NSubstitute-mocked dependencies).
4. `feat(infra)`: `RefreshToken` EF configuration + migration `AddRefreshTokens` + `RefreshTokenRepository`.
5. `feat(infra)`: `JwtTokenService` wystawiający access tokeny (Microsoft.IdentityModel.Tokens already referenced in Api).
6. `feat(api)`: `AuthController` z 3 actions, request/response DTOs, registration in Program.cs.
7. `docs(api)`: Postman collection (register + login + refresh) + smoke instructions.

**Zaplanowane sesje 1.4 B/C/D** (poza scope sesji A):
- B — Quiz endpoints (Categories/Quizzes/Attempts controllers, 6 endpointów).
- C — Cross-cutting: ProblemDetails exception middleware + Swagger UI z JWT auth + CORS dla Vite.
- D — API integration tests via `WebApplicationFactory<Program>` + pełen Postman collection + E2E smoke.

---

## 2026-06-03 — Iteration 1.4 session D: integration tests + Newman/CI smoke runner

**Co zrobione:**
- Nowy projekt `tests/TechQuiz.Api.Tests` (`Microsoft.NET.Sdk.Web`) bootujący prawdziwy host przez `WebApplicationFactory<Program>` przeciw Testcontainers `postgres:16-alpine`, kontener współdzielony przez run xUnit collection fixture. `Program.cs` wystawia `public partial class Program;`.
- Sześć testów integracyjnych, wszystkie zielone:
  - `HealthEndpointTests` — `GET /health` → 200.
  - `AuthorizationTests` — 401 bez tokena na `/api/categories` i `/api/attempts`, 200 z tokenem demo.
  - `AuthEndpointsTests` — rejestracja z błędnym payloadem → 400 `application/problem+json` z per-field `errors` dla `Email`/`Password`.
  - `QuizFlowTests` — pełen E2E: login → categories → start (asercja **braku `iscorrect`** — Hard Rule #4) → answer ×N → complete → re-fetch result.
- Newman runner: `docs/postman/package.json` ze skryptem `npm run smoke` + workflow `.github/workflows/api-smoke.yml` na `workflow_dispatch`.

**Decyzje:**
- **Override konfiguracji przez zmienne środowiskowe, nie `AddInMemoryCollection`.** `WebApplicationFactory` prowadzi minimal hosting przez `DeferredHostBuilder`, więc overe'y `ConfigureAppConfiguration` są ponownie nadpisywane przez appsettings aplikacji. `ConnectionStrings__DefaultConnection` i `Jwt__SigningKey` jako env vary wygrywają z appsettings i user-secrets. Ustawiane w `InitializeAsync`, czyszczone w `DisposeAsync`.
- **Factory migruje przed bootem hosta.** Seeder startowy (gated `IsDevelopment`) nie aplikuje migracji — factory robi `MigrateAsync` na osobnym `AppDbContext` zanim host wstanie i zacznie seedować.
- **Newman jako `workflow_dispatch`, nie per-PR gate.** Flow jest już pokryty in-process przez `WebApplicationFactory` E2E; wartość workflowa to realny HTTP stack z tą samą kolekcją, którą deweloperzy odpalają ręcznie. Migracje w CI z `ASPNETCORE_ENVIRONMENT=Production` (seeder off, bo nie ma design-time factory), start API z `Development` (seeduje dane demo).
- **Bez zmian w CI dla testów integracyjnych.** Istniejący job `backend` (`dotnet test TechQuiz.sln`) sam je podnosi — Testcontainers działa na `ubuntu-latest`.

**Weryfikacja:**
- `dotnet test tests/TechQuiz.Api.Tests` → 6/6 zielonych przeciw Testcontainers Postgres.
- `npm run smoke` przeciw stackowi docker → 9 requestów, 0 failures.

**Punkt wznowienia:**
Iteracja 1.4 zamknięta — kamień milowy backendu MVP. Cztery commity sesji D na `feat/iteration-1.4-api-tests` (#83 harness+health, #84 integration coverage, #85 Newman runner, + ten docs). Następny krok: push gałęzi i jeden PR zamykający iterację 1.4. Kolejne iteracje (1.5–1.8) to frontend — nie zaczynać bez potwierdzenia.

> Iteracja 1.4 domknięta: PR #87 zmergowany do `master` jako `7bb402e`; drobne sprzątanie `API_PID` w smoke workflow (#88).

---

## 2026-06-04 — Iteration 1.5 session A: React foundation (design tokens + theme)

**Kontekst:** `web/` było zescaffoldowane w Phase 0 (React 19 + Vite 8 + Tailwind 3.4 z pustym theme, goły `App.tsx`). Iterację 1.5 dzielimy na 4 sesje: A — fundament (tokeny + theme), B — auth plumbing (+ backendowy refresh-cookie slice), C — routing + AppShell, D — strony Login/Register pod mockupy.

**Co zrobione:**
- **Design tokeny** w `web/src/index.css`: semantyczne CSS variables (`--bg-*`, `--text-*`, `--accent*`, `--border-*`, `--success/warning/danger`) z `docs/ARCHITECTURE.md`. Dark w `:root`, light jako override na `[data-theme="light"]`.
- **Fonty** Geist (400/600) + JetBrains Mono (400) z Google Fonts w `index.html` (preconnect + stylesheet). Statyczny `data-theme="dark"` na `<html>` — strona renderuje themowana zanim React zamontuje.
- **Mapowanie Tailwind** (`tailwind.config.js`): tokeny → utilities (`bg-surface`, `text-secondary`, `font-mono`, …). Kolory borderów w `borderColor` (nie `colors`), żeby `border-default`/`border-strong`/themowany `border` czytały się czysto zamiast `border-border-default`.
- **Runtime motywu** (`web/src/theme/`): `ThemeProvider` (init z localStorage → fallback `prefers-color-scheme` → default dark; reflect na `data-theme`; persist), hook `useTheme`, `ThemeToggle` (inline SVG sun/moon) w `components/ui/`. Context/provider/hook w osobnych modułach, by Fast Refresh nie krzyczał (`react-refresh/only-export-components`).

**Decyzje:**
- **Brak nowych zależności npm w sesji A.** Fonty przez `<link>` Google Fonts (zgodnie z ARCHITECTURE), motyw to czysty React. Router/axios/react-query/rhf/zod/sonner instalujemy w sesjach, które ich faktycznie używają (Hard Rule #6 — uzasadnienie depów w PR).
- **`data-theme` jako nośnik motywu**, nie klasa `.light`. Pogodzone z istniejącym `darkMode: [data-theme="dark"]` w configu i zadaniem 2 iteracji. ARCHITECTURE opisuje `.light`, ale całość i tak jedzie na CSS variables — utility czytają zmienną, więc atrybut wystarcza.

**Weryfikacja:**
- `pnpm build` (tsc -b + vite build) → zielone. `pnpm lint` (eslint) → 0 błędów.
- `pnpm dev` → serwuje HTTP 200, brak błędów runtime; serwowany HTML ma `data-theme="dark"` + linki do fontów. **Wizualny test toggla w przeglądarce nie był wykonany z tego środowiska** — do potwierdzenia ręcznie przez ownera.

**Punkt wznowienia:**
Branch `feat/iteration-1.5-react-foundation`, 2 commity (#89 tokeny/fonty/mapowanie, #90 theme provider/toggle) + ten docs. Następny krok: PR sesji A. Potem **sesja B** — backendowy slice refresh-cookie (`AuthController` ustawia `HttpOnly; SameSite; Secure` cookie i czyta refresh z cookie) + axios client z refresh-on-401 + `AuthContext` + React Query. Uwaga scope: obecnie API zwraca refresh w body JSON — to wymaga zmiany pod model bezpieczeństwa z DoD.

---

## 2026-06-05 — Iteration 1.5 session B: auth plumbing (refresh-cookie slice + axios + AuthContext)

**Kontekst:** Sesja A (fundament) zmergowana jako PR #92 (`990ff97`). Sesja B realizuje model bezpieczeństwa z DoD: **memory-only JWT + HttpOnly refresh cookie**. Przed nią API zwracało refresh token tylko w body JSON, a `/refresh` czytał go z body — XSS-podatne, gdyby front trzymał refresh w JS. Sesja przenosi nośnik refresh tokena do HttpOnly cookie i buduje całą frontendową hydraulikę auth.

**Co zrobione (4 atomic commity):**

1. `feat(api): set refresh token in HttpOnly cookie` (`94b2975`, #93)
   - `AuthController`: po Register/Login/Refresh ustawia cookie `refresh_token` przez `SetRefreshCookie(tokens)` — `HttpOnly=true`, `Secure=!IsDevelopment()`, `SameSite=Strict`, `Path=/api/auth/refresh`, `Expires=RefreshTokenExpiresAt`. Path-scoped: cookie leci tylko na endpoint refresh, nie na każde żądanie.
   - `RefreshRequest(string? RefreshToken)` — refresh w body teraz **opcjonalny** (`[FromBody(EmptyBodyBehavior.Allow)]`). Browser carry'uje go w cookie (puste body), klienci bez cookie jar (Postman/testy) wciąż mogą wysłać w body. `Refresh` czyta `Request.Cookies[...] ?? request?.RefreshToken`.
   - `RefreshCookieTests` (in-memory `WebApplicationFactory`): login ustawia httponly cookie path-scoped → refresh z pustym body czyta cookie → 200; refresh bez cookie i bez body → 401.

2. `feat(web): axios api client + React Query setup` (`5757239`, #94)
   - `web/src/lib/api-client.ts`: instancja axios (`baseURL` z `VITE_API_BASE_URL` ?? `http://localhost:8080`, `withCredentials:true`). Module-scoped `accessToken` (memory-only, nie localStorage). Request interceptor dokłada `Bearer`. `refreshAccessToken()` z jednym współdzielonym `refreshPromise` (deduplikacja równoległych 401). Response interceptor: retry raz na 401 (`_retry` guard, skip dla samego refresh-calla), a po porażce refresh czyści token i woła `onRefreshFailure`.
   - `web/src/lib/query-client.ts`: `QueryClient` (`staleTime 30s`, `retry 1`, bez refetch-on-focus). Nowe depy: `axios`, `@tanstack/react-query` (uzasadnione w stacku CLAUDE.md).

3. `feat(api): logout endpoint clears the refresh cookie` (`6b6aa05`, #95)
   - `POST /api/auth/logout` → `Response.Cookies.Delete(refresh_token, ...)` z tymi samymi atrybutami (Path musi się zgadzać, inaczej przeglądarka nie skasuje) → 204. Bez tego HttpOnly cookie zostałoby i po reloadzie cicho re-auth'owało. Rewokacja tokena w DB odroczona (spójne z długiem MVP #66/#68).
   - Dodatkowy test w `RefreshCookieTests`: login → logout (204, Set-Cookie z `expires=1970`) → refresh → 401.

4. `feat(web): auth context with login, register, logout, and silent refresh` (`0e10db3`, #96)
   - `features/auth/`: `types.ts` (`AuthTokens`, `User`), `jwt.ts` (`decodeUserFromToken` — base64url decode payloadu, czyta `sub`+`email`), `auth-context.ts` (`AuthStatus` loading/authenticated/unauthenticated), `auth-provider.tsx`, `use-auth.ts`.
   - `AuthProvider`: `login`/`register` POST-ują i `applyAccessToken` (set memory token + decode user). `logout` POST `/logout` finally `clearSession`. Effect rejestruje `setOnRefreshFailure(clearSession)` — gdy interceptor 401 nie odświeży, React leci do unauthenticated. Bootstrap effect z `useRef` guardem (StrictMode double-invoke) robi silent `refreshAccessToken()` na starcie — wracający user z ważnym refresh-cookie zostaje wciągnięty bez logowania.
   - `main.tsx`: drzewo `QueryClientProvider > ThemeProvider > AuthProvider > App`. `App.tsx` ma **tymczasowy** wskaźnik `auth: {status}` obok ThemeToggle (zastąpiony routingiem w sesji C).

**Decyzje:**
- **Cookie jako nośnik, body jako fallback.** Zamiast twardo wymagać cookie, `/refresh` akceptuje też body — żeby Postman i integracyjne testy API (bez cookie jar) dalej działały. Browser i tak nigdy nie zobaczy refresh tokena w JS (memory-only access token + HttpOnly refresh).
- **Path-scoped cookie (`/api/auth/refresh`).** Refresh token nie wisi na każdym żądaniu — mniejszy attack surface, leci tylko tam gdzie potrzebny.
- **Rewokacja w DB odroczona.** Logout czyści cookie po stronie klienta, ale nie unieważnia tokena w bazie — świadomy dług MVP (#66/#68), nie regres. Pełna rotacja/rewokacja w późniejszej iteracji.
- **Memory-only access token.** Zgodnie z gotcha w CLAUDE.md — żaden JWT nie ląduje w localStorage. Token żyje w module-scoped zmiennej; po refreshu strony znika i jest odtwarzany przez silent refresh.

**Weryfikacja:**
- `dotnet build` → 0 warnings. `dotnet test` → **9/9 zielone** (`RefreshCookieTests` na realnie skompilowanym branchu przez `WebApplicationFactory<Program>`).
- `pnpm build` + `pnpm lint` → zielone.
- **Runtime check na żywym dockerowym API** (obraz przebudowany z brancha — `docker compose up -d --build api`): pełny łańcuch cookie potwierdzony curl-em →
  - register → `Set-Cookie: refresh_token=…; path=/api/auth/refresh; samesite=strict; httponly; expires=<future>`,
  - refresh z cookie + pustym body → **200**,
  - logout → **204**, `Set-Cookie` z `expires=1970` (cookie skasowane),
  - refresh po logout → **401**.
  - CORS preflight z `Origin: http://localhost:5173` → 204 z `Access-Control-Allow-Credentials: true` + `Allow-Origin: http://localhost:5173` (cross-origin cookie flow z dev serwera React przejdzie).
- **Przeglądarkowy przepływ React (klik przez Login UI) NIE był weryfikowany** — brak headless browsera w tym środowisku; UI loginu/rejestracji powstaje w sesji D. Backend i hydraulika pokryte testami + curl-em.

**Punkt wznowienia:**
Branch `feat/iteration-1.5-auth-plumbing`, 4 commity (#93–#96) + ten docs. Następny krok: PR sesji B (zamyka #93/#94/#95/#96 + docs). Potem **sesja C** — React Router + `RequireAuth` + `AppShell` (topbar route-aware, ukryty na `/quiz/:id`), zastąpienie tymczasowego wskaźnika auth w `App.tsx`.

**Review PR #98 (owner) — obsłużone w `d0e4bd5`:**
- #2 `vite-env.d.ts` `type` → `interface` (declaration-merging z `vite/client` — carve-out z CLAUDE.md).
- #3 `jwt.ts` komentarz "display-only, no signature check" (nie używać `user.id` jako authz po stronie klienta).
- #4 `jwt.ts` dekodowanie payloadu przez `TextDecoder` zamiast `atob` (UTF-8 safe — non-ASCII email).
- #1 (medium) refresh token wciąż w body JSON: **świadomy defer** — Postman/Newman zależą od `body.refreshToken`, a czyste usunięcie rusza `AuthTokensDto` (Application) + kolekcję Postmana. Złagodzony komentarz w `AuthController`; pełne wyjęcie śledzone w **#99**.
PR #98 zmergowany (`squash`), issues #93–#97 + #100 zamknięte, #99 otwarte jako follow-up.

---

## 2026-06-05 — Iteration 1.5 session C: routing + app shell

**Kontekst:** Sesja B (auth plumbing) zmergowana (PR #98). Sesja C realizuje zadania 5–6 planu iteracji: React Router + `RequireAuth` + route-aware `AppShell`. To zdejmuje tymczasowy wskaźnik `auth: {status}` z `App.tsx` i daje realną nawigację pod strony, które wypełnią iteracje 1.6/1.7.

**Co zrobione (3 atomic commity):**

1. `feat(web): add React Router with routing skeleton and placeholder pages` (`1c76a90`, #101)
   - `react-router-dom` **przypięty do v6** (6.30.4). pnpm domyślnie zaciągnął v7.17.0 — cofnięte, bo ADR-007 i plan iteracji mówią v6, a używane API (`BrowserRouter`/`Routes`/`Route`/`Navigate`/`Outlet`/`NavLink`/`useMatch`) jest w obu identyczne; brak powodu na cichy bump wbrew udokumentowanej decyzji.
   - `App.tsx` jako host routera: trasy `/login`, `/register`, `/categories`, `/quiz/:id`, `/result/:attemptId`, catch-all → `/categories`. Znika tymczasowy wskaźnik auth z sesji B.
   - Page-stuby w feature folderach: `features/categories`, `features/quiz`, `features/results` (treść w 1.6/1.7). Login/Register w `features/auth` jako **minimalne, ale funkcjonalne** stuby (form + `useAuth().login/register`) — żeby guard i przepływ auth dało się przejść end-to-end już w C; pełne UI pod mockupy w sesji D.

2. `feat(web): RequireAuth guard for protected routes` (`7856d88`, #102)
   - `RequireAuth` jako layout route owijający trasy chronione. `status === 'loading'` (bootstrap silent refresh) → `FullPageSpinner` zamiast redirectu, żeby wracający user z ważnym refresh-cookie **nie mignął** ekranem logowania. `unauthenticated` → `<Navigate to="/login">` z `state.from = ścieżka`, którą `LoginPage` czyta i wraca po zalogowaniu.
   - Nowy prymityw UI `FullPageSpinner` (`components/ui/`).

3. `feat(web): route-aware AppShell topbar` (`434af71`, #103)
   - `AppShell` jako layout route zagnieżdżony w `RequireAuth`, wg `docs/mockups/categories-*.html` + ADR-014: logo (kafel "T" + wordmark), aktywny `Categories` (`NavLink`), wyłączone `Daily review`/`Generate`/`Dashboard`/`History` z badge'ami `soon`, prawa strona theme toggle + avatar.
   - Avatar = inicjały z emaila (`initialsFromEmail`) i **przycisk logout**: `logout()` czyści sesję → `status` → `unauthenticated` → `RequireAuth` sam przekierowuje na `/login` (bez ręcznego `navigate`).
   - **Route-aware**: na `/quiz/:id` (`useMatch`) renderuje sam `<Outlet />` bez topbara — focused quiz screen (gotcha z CLAUDE.md + ADR-014).
   - Tokeny zamiast hardcoded hex z mockupu; `text-white` na pełnym `bg-accent` (brak tokena "on-accent", biały daje poprawny kontrast w obu themach).

**Decyzje:**
- **React Router v6, nie v7.** Zgodnie z ADR-007 i planem; v7 cofnięty mimo że to obecny major. Bump rozważymy świadomie później, nie przy okazji.
- **Login/Register jako funkcjonalne stuby w C.** Kompromis: na tyle działające, by zweryfikować guard/redirect/logout end-to-end teraz, ale celowo surowe — mockupowe UI (split-screen, rhf+zod, demo, sonner) to sesja D, żeby nie mieszać scope.
- **Logout bez ręcznej nawigacji.** Redirect spada naturalnie z `RequireAuth` po flipie statusu — jedno źródło prawdy dla "gdzie ląduje niezalogowany".

**Weryfikacja:**
- `pnpm build` (tsc -b + vite) + `pnpm lint` → zielone po każdym z 3 commitów.
- `pnpm dev` → HTTP 200, bootuje bez błędu runtime; serwuje shell z `#root`.
- **Klikany przepływ w przeglądarce NIE był weryfikowany** (brak headless browsera): logowanie → guard → shell, toggle motywu, ukrycie topbara na `/quiz/:id`, logout→redirect — do potwierdzenia ręcznie przez ownera. Brak mandatu testów front w MVP, więc bar to zielony build+lint.

**Punkt wznowienia:**
Branch `feat/iteration-1.5-routing-shell`, 3 commity (#101–#103) + ten docs. Następny krok: PR sesji C (zamyka #101/#102/#103 + docs). Potem **sesja D** (zadania 7–10) — strony Login/Register dokładnie pod `mockups/login-*.html` (split-screen, gradient hero, "Continue as demo", `react-hook-form` + `zod`), loading states, błędy inline + `sonner` na nieoczekiwane.

---

## 2026-06-05 — Iteration 1.5 session D: Login/Register UI under mockup

**Kontekst:** Sesja C (routing + shell) zmergowana (PR #105). Sesja D realizuje zadania 7–10 planu: zastąpienie funkcjonalnych stubów Login/Register pełnym UI pod `docs/mockups/login-dual-theme.html` (split-screen, gradient hero, `react-hook-form` + `zod`, "Continue as demo", `sonner`). To domyka iterację 1.5.

**Co zrobione (3 atomic commity):**

1. `chore(web): add react-hook-form, zod, sonner; mount themed Toaster` (`f439580`, #106)
   - Deps: `react-hook-form`, `zod`, `@hookform/resolvers`, `sonner`.
   - `ThemedToaster` (`components/ui/`) czyta motyw z `useTheme` i podaje go `<Toaster>` — toasty trzymają się tego samego `data-theme` co reszta UI. Zamontowany w `main.tsx` wewnątrz `ThemeProvider`, obok `<App/>`.

2. `feat(web): split-screen login page matching mockup` (`b3ec1aa`, #107)
   - `AuthLayout` — wspólna rama split-screen: lewa kolumna (header logo "T" + `ThemeToggle`, slot na formularz, stopka `© 2026 TechQuiz · v0.1.0`), prawa = `AuthHero`. Hero `hidden lg:block` — poniżej lg formularz bierze całą szerokość.
   - `AuthHero` — dekoracyjny panel: dwa bloby `radial-gradient` (inline style, fiolet/indygo), siatka `.auth-grid` (theme-aware, 40px) i karty z `.auth-float-*` (gentle float, wyłączone przy `prefers-reduced-motion`). Statyczne dane demo (C# Advanced 87% + ASP/EF/SQL) — sprzedaje produkt wizualnie.
   - `LoginPage` na `react-hook-form` + `zod`: walidacja email/hasło, przycisk **"Continue as demo"** (loguje seedowanym `demo@techquiz.local`), osobne loading per-przycisk, **401 → błąd inline** na polu hasła, reszta → `toast.error`.
   - `index.css`: token `--auth-grid-line` (dla obu themów), `.auth-grid`, `@keyframes auth-float-hero/-stack` + reduced-motion guard.

3. `feat(web): register page with confirm-password validation` (`a859b7a`, #108)
   - `RegisterPage` reużywa `AuthLayout`: `react-hook-form` + `zod` z polem **confirm password** i `refine()` sprawdzającym zgodność haseł (min. 8 znaków). 4xx (email zajęty / słabe hasło) → inline, reszta → toast. Krzyżowa nawigacja do `/login`.

**Decyzje:**
- **Brak opacity-modifierów na tokenach `var()`.** Kolory w `tailwind.config.js` to gołe `var(--token)` bez kanału `<alpha-value>`, więc `bg-surface/75`, `border-accent/35` są **cicho ignorowane**. `AuthHero` i inputy używają solidnych tokenów (`bg-elevated` vs `bg-surface` dla kontrastu) zamiast półprzezroczystości z mockupu; focus-ring solidny `ring-accent`. Pełny refaktor do alpha-tokenów ruszyłby fundament z sesji A — poza scope 1.5.
- **"Forgot password?" i "Keep me signed in" — kosmetyka.** Link "Forgot password?" pokazuje toast "later phase"; checkbox "Keep me signed in" z mockupu **pominięty**, bo żywotność refresh-cookie jest ustalana po stronie serwera (stała), więc kontrolka byłaby myląca.
- **Rozróżnienie błędów: inline vs toast.** 401/4xx to wina inputu użytkownika → komunikat przy polu; sieć/5xx to nie jego wina → `sonner` toast. Spójne między Login i Register.

**Weryfikacja:**
- `pnpm build` (tsc -b + vite) + `pnpm lint` → zielone po każdym z 3 commitów.
- **Klikany przepływ w przeglądarce NIE był weryfikowany** (brak headless browsera w tym środowisku): wygląd split-screen w dark/light, "Continue as demo", walidacja rhf+zod, inline-401 vs toast — do potwierdzenia ręcznie przez ownera. Brak mandatu testów front w MVP; bar to zielony build+lint.

**Punkt wznowienia:**
Branch `feat/iteration-1.5-auth-ui`, 3 commity (#106–#108) + ten docs. Następny krok: PR sesji D (zamyka #106/#107/#108 + docs) — **domyka iterację 1.5**. Potem **iteracja 1.6**: siatka kategorii + pełny runner quizu (realna luka do "robienia zadań z pierwszego tematu").

**Review PR #110 (owner) — obsłużone w `31b4eed` + `32143f1`:**
- #1 (medium) obsługa błędów rejestracji: zamiast sztywnego zgadywania czytam `errors` z ProblemDetails. Niuans: `RegistrationFailedException` (realna ścieżka — zajęty email / słabe hasło) serializuje **płaską `string[]`**, a FluentValidation `{ pole: string[] }`. `fieldErrorsFromProblem` normalizuje oba i kieruje komunikaty „password…" na pole hasła, resztę na email. Ścieżka inline zawężona z `< 500` do `400 || 409`.
- #2 (low-med) a11y: `aria-invalid` + `aria-describedby` na wszystkich polach obu formularzy + `id` na każdym `<p>` błędu.
- #3 (low) dryf nazwy mockupu: DoD i drzewo mockupów w CLAUDE.md wskazują na realny `login-dual-theme.html`.
- #4 (nit) hardcoded rgba w `AuthHero`: świadomy defer (tokeny bez kanału alpha), dodany komentarz honesty.
PR #110 zmergowany (`squash`, `f20890c`), issues #106–#109 + #111/#112 zamknięte. **Iteracja 1.5 done.**

---

## 2026-06-05 — Iteration 1.6 session A: Categories grid + start-a-quiz

**Kontekst:** Iteracja 1.5 zamknięta (PR #110). 1.6 to najcięższa iteracja (10 zadań, najwięcej UX) — pocięta na 3 sesje/PR-y: **A** kategorie + start, **B** rdzeń runnera (mini-header, pytanie, stan selected wg ADR-015), **C** interakcje + persystencja (klawiatura, submit odpowiedzi, modal wyjścia, complete→result). Ta sesja realizuje zadania 1–2.

**Realny kontrakt API (zweryfikowany w kodzie, nie z pliku iteracji):**
- `GET /api/categories` → `CategoryDto[] { id, name, description, iconCode, questionCount, userBestScore }`.
- `POST /api/quizzes/start` body `{ categoryId }` → `QuizSessionDto { attemptId, questions[] }`; `questions[] = { id, type, difficulty, text, options[{ id, text, orderIndex }] }` — bez `isCorrect` (hard rule #4) i bez `explanation`.
- `POST /api/quizzes/{attemptId}/answer` body `{ questionId, selectedOptionId|null }` → 204. `POST .../complete` → `QuizResultDto`.
- **Brak `JsonStringEnumConverter` w API** → enumy lecą jako liczby (Difficulty Easy=0/Medium=1/Hard=2). Front mapuje liczbowo (`features/quiz/types.ts`).

**Co zrobione (1 commit):**

1. `feat(web): categories grid with start-a-quiz wiring` (`b190ee8`, #113)
   - Warstwa API/hooków: `features/categories/{api.ts,use-categories.ts}` (`useQuery(['categories'])`), `features/quiz/{types.ts,api.ts,query-keys.ts,use-start-quiz.ts}`.
   - `CategoriesPage` wg `mockups/categories-*.html`: responsywna siatka 3-kol, **active vs „Coming soon" z `questionCount > 0`** (bez hardcode'u), pasek best-score na aktywnych kartach, stany loading/error z retry. Strona daje własny kontener (`AppShell` renderuje goły `<Outlet/>`).
   - `useStartQuiz` = mutacja (start tworzy attempt → write): `POST /start` na klik aktywnej karty → `setQueryData(quizSessionKey(attemptId), session)` → `navigate('/quiz/:attemptId')`. **Bez tranzytowej trasy `/quiz/start`** z planu — mutacja na kliknięciu jest czystsza. QuizPage odczyta cache w sesji B.

**Decyzje:**
- **Active/coming-soon wyprowadzone z danych (`questionCount`), nie z hardcoded listy.** Gdy backend zaseeduje kolejną kategorię, karta sama staje się grywalna — zero zmian we froncie.
- **Start jako mutacja + seed cache, nie trasa pośrednia.** Plan proponował `/quiz/start?categoryId=X`; mutacja na kliknięciu unika migającej trasy i daje QuizPage gotowe pytania z cache (jeden round-trip).
- **Enumy liczbowo na froncie.** Konwerter string-enum w API ruszyłby testy/Postmana — świadomy defer poza scope 1.6.

**Weryfikacja:**
- `pnpm build` (tsc -b + vite) + `pnpm lint` → zielone.
- **Klikany przepływ NIE był weryfikowany** (brak headless browsera): siatka w dark/light, klik karty → start → wejście w `/quiz/:attemptId`, stany loading/error — do potwierdzenia ręcznie przez ownera (`pnpm dev`, demo-login → Categories). Backend pokryty testami z wcześniejszych iteracji.

**Punkt wznowienia:**
Branch `feat/iteration-1.6-categories`, 1 commit (#113) + ten docs. Następny krok: PR sesji A (zamyka #113 + docs). Potem **sesja B** — `QuizPage` czyta sesję z cache pod `quizSessionKey`, mini-header (progress + exit X), `Question` (4 opcje, prefiksy mono 1–4), stan selected wg ADR-015 (border + filled prefix + glow).

**Code review PR #115 — poprawki (1 commit):**
- **#1 reload nieodzyskiwalny (medium)** → wybrana opcja tania: notatka „Known MVP limitation" w pliku iteracji 1.6. Cache sesji jest tylko w pamięci (`setQueryData` bez `queryFn`), brak `GET` dla pytań trwającego attempt. QuizPage w sesji B wykryje cache-miss i przekieruje na `/categories`. Pełny resume (`GET /api/quizzes/{attemptId}/questions` bez `IsCorrect`) odłożony do późniejszej iteracji.
- **#2 typy enumów (nit)** → `QuizQuestion.type/difficulty` zacieśnione z gołego `number` do `QuestionTypeValue`/`DifficultyValue`; dodany `QuestionType` const (0=MultipleChoice, 1=CodeOutput) — przyda się w sesji B do rozgałęzienia renderu.
- **#3 aktywna-niezagrana 0% (low)** → mockup pokazuje „Not started" dla niezagranych; aktywna karta z `userBestScore === 0` też renderuje „Not started" zamiast paska 0%.
- **#4 spójność kluczy (nit)** → dodana fabryka `categoriesKey()` (`features/categories/query-keys.ts`), `useCategories` jej używa — symetrycznie do `quizSessionKey`.
- Weryfikacja: `tsc -b` + `eslint .` zielone. Klikany przepływ wciąż do ręcznego potwierdzenia przez ownera.

## 2026-06-05 — Iteration 1.6 session B: Quiz runner shell (mini-header + question + selected state)

**Kontekst:** PR #115 zmergowany (`3cc5b0d`). Sesja B realizuje zadania 3–6: `QuizPage` (rdzeń runnera), mini-header, komponent pytania, stan selected wg ADR-015. Branch `feat/iteration-1.6-quiz-runner`.

**Referencje zweryfikowane:** `mockups/quiz-multiple-choice-dark.html` (struktura full-screen, kolory), ADR-015 (3 zmiany stanu selected: violet border + filled prefix + 3px outer glow; badge trudności emerald/amber/red @10%; full-screen z mini-headerem; bez feedbacku do Result). Trasa to `/quiz/:id` (param `id`, nie `attemptId`). `AppShell` na tej trasie zwraca goły `<Outlet/>` — QuizPage sam bierze pełny ekran (`min-h-screen flex flex-col`).

**Co zrobione (1 commit):**
- `QuizPage` przepisany ze stuba na pełny runner: czyta sesję z cache pod `quizSessionKey(id)`; **cache-miss → `<Navigate to="/categories" replace />`** (realizacja decyzji #1 z review). Lokalny stan: `currentIndex`, `answers: Record<questionId, optionId>`. Mini-header (nazwa kategorii · licznik, pasek progresu `transition-[width] duration-300`, przycisk exit X). Pytanie: badge trudności + treść. 4 opcje z prefiksami mono 1–4. Stopka: podpowiedź klawiszy (`<Kbd>`) + przycisk Next/„Submit quiz" (disabled dopóki brak odpowiedzi). Nawigacja **tylko w przód** (mockup nie ma Back).
- **Stan selected wg ADR-015** — 3 zmiany jednocześnie: `border-accent`, prefiks `bg-accent text-white`, glow `shadow-[0_0_0_3px_rgba(139,92,246,0.15)]`. Tło opcji pozostaje `bg-surface`.
- **Nazwa kategorii do mini-headera:** `QuizSessionDto` jej nie niesie. Zamiast pola w backendzie — nowy typ `QuizRunnerSession = QuizSession & { categoryName: string }`; `useStartQuiz` przyjmuje teraz `{ id, name }`, woła `startQuiz(id)` i seeduje cache wzbogacony o `categoryName`. `CategoriesPage` woła `start.mutate({ id, name })`, `startingId = start.variables?.id`.

**Decyzje:**
- **Nazwa kategorii przepuszczona przez mutację → cache, bez zmiany backendu.** Tańsze niż nowe pole w DTO + handler + test; nazwa i tak jest pod ręką na klik karty.
- **Badge trudności:** kolor tekstu z tokenu (`text-success/-warning/-danger`, flipuje w light), tło literalnym `rgba(...,0.1)` — tokeny nie mają kanału alpha, więc `bg-warning/10` jest cicho gubione (ten sam gotcha co w auth-hero). Glow też arbitrary value (`shadow-[...]`) z literalnym rgba — dokładnie jak mockup.
- **Tekst opcji zwykły, nie violet-mono.** Mockup pokazuje opcje jako `<code>` (bo to słowa kluczowe C#); realne odpowiedzi bywają prozą — prefiks/stan selected wg ADR-015, ale treść opcji to zwykły `text-[14px]`.
- **Interim wiring (do podmiany w sesji C):** exit X → `navigate('/categories')` (sesja C owinie w modal potwierdzenia); „Submit quiz" na ostatnim pytaniu → `navigate('/result/:id')` (sesja C dołoży `POST /complete` przed nawigacją; Result to wciąż stub do 1.7). Nawigacje zostają, sesja C tylko dokłada bramki — nie throwaway.

**Poza zakresem (sesja C):** klawiatura (1–4/Enter/Esc), persystencja `POST /answer`, modal wyjścia (forfeit), `POST /complete` → Result.

**Weryfikacja:**
- `pnpm build` (tsc -b + vite, 229 modułów) + `eslint .` → zielone.
- **Klikany przepływ NIE weryfikowany** (brak headless browsera): full-screen w dark/light, klik opcji → stan selected (3 zmiany), pasek progresu, Next disabled/enabled, exit X → Categories — do potwierdzenia ręcznie przez ownera (`pnpm dev`, demo-login → Categories → klik aktywnej karty → quiz).

**Punkt wznowienia:**
Branch `feat/iteration-1.6-quiz-runner`, 1 commit + ten docs. Następny krok: PR sesji B. Potem **sesja C** — handler klawiatury (`useEffect` + keydown), mutacja `POST /answer` (idempotentna), modal wyjścia (rekomendacja: `@radix-ui/react-dialog`), `POST /complete` → `/result/:id`.

## 2026-06-05 — Iteration 1.6 session C: interactions + persistence (keyboard, answer, exit modal, complete)

**Kontekst:** PR #118 zmergowany (`6c3a46f`). Sesja C zamyka iterację 1.6 (zadania 7–10). Branch `feat/iteration-1.6-quiz-interactions`. Kontrakt zweryfikowany w kodzie: `POST /api/quizzes/{attemptId}/answer` body `{ questionId, selectedOptionId? }` → 204; `POST .../complete` → `QuizResultDto`.

**Nowa zależność:** `@radix-ui/react-dialog` (1.1.16) — modal wyjścia. Sankcjonowana wprost w pliku iteracji („radix-ui Dialog component or shadcn equivalent"), więc bez ADR.

**Co zrobione (1 commit):**
- **API + hooki:** `api.ts` += `submitAnswer(attemptId, questionId, selectedOptionId)` i `completeQuiz(attemptId)`. `use-submit-answer.ts` (mutacja, fire-on-select, toast tylko na błędzie), `use-complete-quiz.ts` (mutacja → `onSuccess` `navigate('/result/:id')`).
- **`ExitQuizDialog`** (radix, kontrolowany `open`/`onOpenChange`/`onConfirm`): „Exit quiz? … Your progress will be lost." + Cancel / „Yes, exit" (danger). Ten sam modal obsługuje X w headerze i Esc.
- **`QuizPage` rozbity na `QuizPage` (lookup cache + redirect) i `QuizRunner` (wszystkie hooki, sesja zawsze ważna)** — eliminuje problem warunkowych hooków po early-return.
- **Klawiatura** (`useEffect` + global `keydown`): 1–4 wybiera opcję, Enter idzie dalej (jeśli odpowiedziano), Esc otwiera modal. Zawieszona gdy modal otwarty (radix przejmuje Esc). `selectAnswer`/`handleAdvance` w `useCallback` (stabilne dzięki destrukturyzacji `.mutate`), więc deps efektu są poprawne bez eslint-disable.
- **Persystencja:** `selectAnswer` optymistycznie ustawia stan + odpala `submitAnswer.mutate` (idempotentny upsert per pytanie — bezpieczny przy zmianie zdania przed Next).
- **Submit:** ostatnie pytanie → `handleAdvance` woła `completeMutate(attemptId)` → nawigacja do `/result/:id`. Przycisk disabled gdy brak odpowiedzi lub `isCompleting` (guard też w Enter — bez podwójnego complete).

**Decyzje:**
- **`QuizPage`/`QuizRunner` split** — czystsze niż guardy wokół hooków; runner dostaje już zwalidowaną sesję.
- **Fire-on-select zamiast zapisu przy Next** — zgodne z zadaniem 8 i ADR-015 („answer can be changed before Next"); backend upsertuje, więc wielokrotny POST jest idempotentny.
- **`completeQuiz` nie konsumuje odpowiedzi** — Result (1.7) pobierze breakdown przez `GET /result`; tu tylko complete + nawigacja.
- **Znana, zaakceptowana krawędź:** Enter na sfokusowanym przycisku Next (Tab) mógłby teoretycznie podwoić advance; w praktyce 1–4 nie przenoszą fokusu (zostaje na body), więc Enter→advance działa raz. Guard `isCompleting` chroni complete. MVP-OK.

**Weryfikacja:**
- `pnpm build` (tsc -b + vite, 286 modułów) + `eslint .` → zielone.
- **Klikany/klawiszowy przepływ NIE weryfikowany** (brak headless browsera): 1–4 → select, Enter → next, Esc → modal, „Yes, exit" → Categories, ostatnie pytanie → Submit → `/result/:id`, błąd answer/complete → toast — do potwierdzenia ręcznie przez ownera.

**Punkt wznowienia:**
Branch `feat/iteration-1.6-quiz-interactions`, 1 commit + ten docs. Iteracja 1.6 **zamknięta** (wszystkie DoD ✓). Następny krok: PR sesji C → merge → iteracja **1.7** (Result page — `GET /result`, breakdown wg `mockups/result-*.html`).

**Code review PR #120 — poprawki (1 commit, ten sam branch przed mergem):**
- **#1 wyścig complete vs zapis (medium)** → zapis odpowiedzi na `mutateAsync`, bieżący zapis trzymany w `pendingSaveRef`; `handleAdvance` na ostatnim pytaniu **awaitu­je `pendingSaveRef` przed `completeAsync`**. Jeśli zapis padł — abort completu (nie zliczamy bez ostatniej odpowiedzi).
- **#2 nieaktualny optimistic state przy błędzie zapisu (medium)** → `selectAnswer` na `save.catch` **rolluje** zaznaczenie (tylko jeśli user nie wybrał w międzyczasie innej opcji dla tego pytania) + toast „pick it again"; przez rollback Next się dezaktywuje. Toast usunięty z hooka `useSubmitAnswer` (runner jest jedynym właścicielem błędu).
- **#3 podwójny complete (low–medium)** → `completingRef` (`useRef`) jako synchroniczny latch wokół completu — zamyka okno między dwoma synchronicznymi zdarzeniami przed re-renderem. Reset latcha tylko na błędzie (sukces → nawigacja/unmount).
- **#5 brak fallbacku trudności (nit)** → `DIFFICULTY_META[difficulty] ?? Medium` — enum spoza 0–2 nie white-screen'uje runnera.
- **#4 forfeit tylko po stronie klienta (low)** → nota w pliku iteracji: exit to `navigate('/categories')`, brak endpointu forfeit, attempt zostaje in-progress w DB (ten sam dług co reload z sesji A). Tylko dokumentacja.
- Weryfikacja: `pnpm build` (286 modułów) + `eslint .` zielone. Klikany/klawiszowy przepływ wciąż do ręcznego potwierdzenia przez ownera (w tym: wymuszony błąd `/answer` → rollback + toast + Next disabled; szybki submit na ostatnim pytaniu → ostatnia odpowiedź policzona).


---

## 2026-06-07 — Iteration 1.7 Session A: denormalizacja ScorePercentage (backend)

**Kontekst:** Ekran wyniku (mockup `result-*.html`) potrzebuje nazwy kategorii, best-score i porównania „+X% from your last attempt". Przy analizie odkryto, że `ICategoryRepository.GetUserBestScoresAsync` to **placeholder zwracający pusty słownik** (dlatego siatka kategorii pokazuje wszędzie 0%). Komentarz w kodzie wprost wskazywał właściwą poprawkę: zdenormalizować procent wyniku na `QuizAttempt` przy `Complete` i agregować przez `MAX() GROUP BY`. Owner wybrał tę drogę zamiast wariantu pragmatycznego (wątek cache na froncie).

**Co zrobione (3 atomic commits, branch `feat/iteration-1.7-score-denormalization`):**

1. `feat(domain): denormalise ScorePercentage on QuizAttempt at completion` (#122)
   - `QuizAttempt.ScorePercentage` (nullable, null do ukończenia), ustawiane w `Complete(completedAt, scorePercentage)`.
   - `CompleteQuizCommandHandler` liczy `Score` **przed** `Complete` i przekazuje procent.
   - Zmiana sygnatury `Complete` — zaktualizowani wszyscy wywołujący (1 produkcyjny + 7 w testach). TDD na encji.

2. `feat(infra): persist score percentage and add best/last-score queries` (#123)
   - Mapowanie EF + migracja `AddQuizAttemptScorePercentage` (kolumna `score_percentage double precision`, nullable).
   - Prawdziwe `GetUserBestScoresAsync` — `JOIN` attempts→quizzes po `QuizId`, `GROUP BY CategoryId`, `MAX(ScorePercentage)`, scoped do usera, omija niezukończone.
   - Nowe `IQuizRepository.GetLastCompletedScoreAsync(user, quiz, excludeAttempt)` — najświeższy ukończony wynik dla quizu z wykluczeniem bieżącego podejścia (null gdy brak). Testy Testcontainers dla obu.

3. `feat(app): expose category, best and previous score on quiz result` (#124)
   - `QuizResultDto` + `CategoryId`, `CategoryName`, `BestPercentage`, `PreviousPercentage`; wpięte w współdzieloną `QuizResultProjection`.
   - Oba handlery (`GetQuizResult`, `CompleteQuiz`) wzbogacają wynik: nazwa kategorii (`GetAllAsync`), best (`GetUserBestScoresAsync`, fallback do bieżącego wyniku), previous (`GetLastCompletedScoreAsync`). W complete best czytany **po** zapisie (zawiera świeżo ukończone podejście).

**Decyzje:**
- **Denormalizacja zamiast liczenia w locie** — best/previous to teraz prosty aggregate/ORDER-BY zamiast re-scoringu odpowiedzi każdego podejścia. Odblokowuje też realne best-score na siatce kategorii (1.6 używała fallbacku 0%).
- **`previous` z wykluczeniem bieżącego podejścia** — w MVP kategoria = jeden quiz, więc „ostatnie podejście" to najświeższy ukończony attempt tego samego `QuizId` poza bieżącym.
- **Pól nie przekazujemy przez cache frontu** — DTO niesie komplet, więc ekran wyniku przeżyje refresh/deep-link (w przeciwieństwie do runnera).

**Weryfikacja:**
- `dotnet build TechQuiz.sln` → zielone, 0 ostrzeżeń.
- Domain 50/50, Application 76/76 (lokalnie).
- **Testy Infrastructure (Testcontainers) NIE uruchomione lokalnie** — brak działającego Dockera; kompilują się, wykona je CI.

**Punkt wznowienia:**
PR sesji A (backend) → po zielonym CI merge. Następnie Sesja B (frontend): `useQuizResult` (`GET /result`) + `ResultPage` wg `mockups/result-*.html`. DoD 8–11 (polish, Lighthouse ≥90, README, demo 90s) — po stronie ownera.

**Domknięcie sesji A:** CI najpierw padło (Backend) — mój helper `SeedCategoryWithQuizAsync` w `QuizRepositoryTests` nadawał kategorii stałą nazwę „Test Category", a nowy test seeduje dwie kategorie → kolizja na unikalnym indeksie `ix_categories_name` (Postgres 23505). Fix: nazwa pochodna od `id` (#127). Po fixie CI zielone, PR #126 zmergowany (squash), zamyka #122–#125 + #127.


---

## 2026-06-07 — Iteration 1.7 Session B: Result screen (frontend)

**Kontekst:** Sesja A wystawiła komplet danych na `GET /api/quizzes/{attemptId}/result` (kategoria, best, previous, per-question z `isCorrect` + explanation). Sesja B renderuje ekran wyniku wg `mockups/result-*.html`. Route `/result/:attemptId` i nawigacja z `useCompleteQuiz` już istniały z 1.6 — był tylko stub strony.

**Co zrobione (2 atomic commits, branch `feat/iteration-1.7-result-screen`):**

1. `feat(web): result data layer — useQuizResult hook + types` (#128)
   - `features/results/`: `types.ts` (QuizResult/ResultQuestion/ResultOption — camelCase, `byDifficulty` jako `Record<string, …>` bo enum-klucze przychodzą jako liczby-stringi), `api.ts` (`fetchQuizResult` → `GET …/result`), `query-keys.ts`, `use-quiz-result.ts` (`useQuery`, `staleTime: Infinity` — wynik ukończonego podejścia jest niezmienny).

2. `feat(web): ResultPage — score hero, metric cards, breakdown, CTAs` (#129)
   - Score hero (gradient violet, duży %, „+X% from your last attempt" / „First attempt" gdy `previousPercentage === null`, pill z etykietą jakościową).
   - 4 metric cards: Correct/Total, Time, Avg/question (Time + Avg **liczone na froncie** z `startedAt`/`completedAt`), Best score.
   - Review: pytania błędne rozwinięte (your answer vs correct + blok explanation z violet border-left), poprawne za toggle „Show N more correct answers"; każdy wiersz osobno rozwijalny (`ReviewRow` z lokalnym stanem, błędne domyślnie otwarte).
   - CTA: „Back to categories" (primary) + „Try again" (reużywa `useStartQuiz` z tym samym `categoryId`/`categoryName`).

**Decyzje:**
- **Tła statusowe jako literalne rgba** (inline `style`), tekst przez tokeny `text-success`/`text-danger`/`text-accent-text` — ta sama konwencja co `quiz-page.tsx` (tokeny kolorów nie mają kanału alfa, więc `bg-*/10` jest po cichu pomijane).
- **„Try again" = `useStartQuiz`** — żadnego nowego hooka; start to mutacja tworząca nowe podejście i nawigująca do `/quiz/:newId`.
- **Brak biblioteki ikon** — inline SVG jak w istniejących komponentach (hard rule #6: bez nowych zależności).

**Weryfikacja:**
- `pnpm tsc --noEmit`, `eslint .`, `pnpm build` (289 modułów) — zielone.
- **Wizualny przebieg w przeglądarce NIE wykonany lokalnie** — ekran wymaga działającego API (Postgres/Docker), którego nie ma w tym środowisku. Render wyniku do potwierdzenia przez ownera w środowisku ze wstałym stackiem (dark + light, golden path + „first attempt" bez porównania).

**Punkt wznowienia:**
PR sesji B → po zielonym CI merge. Pozostałe DoD 1.7: polish pass, Lighthouse a11y ≥90, README (screeny + demo creds), nagranie 90s — po stronie ownera (wymagają przeglądarki/działającego stacku).

---

## 2026-06-08 — Content expansion batch 1: SQL question bank (kategoria #2)

**Kontekst:** Apka po 1.7 jest funkcjonalnie kompletna (login → kategorie → quiz → wynik), ale ma tylko 1 zaseedowaną kategorię ("Unit Testing", 19 pytań). Owner przygotowuje się do interview EPAM (tematy: SQL, EF Core, ASP.NET, C#/.NET, …) i chce używać apki do nauki. Plan: dosypywać kategorie partiami, 1 kategoria = 1 PR, po 20 pytań. Batch 1 = SQL, EF Core, ASP.NET Core, C#/.NET. Start od SQL.

**Co zrobione (branch `feat/sql-question-bank`, issue #132):**
- `SqlQuestions.cs` — bank 20 pytań MultipleChoice, źródło: kurs EPAM moduł 011 (Relational Databases & SQL) graded quizy. Zakres: DBMS/klucze/constraints, relacje (1:N, M:N), normalizacja (1NF/2NF/3NF, OLTP vs OLAP), SELECT/GROUP BY/HAVING z semantyką NULL, INTERSECT, CTE (WITH), DML (INSERT/UPDATE/DELETE/TRUNCATE), TCL, DDL (ALTER TABLE, views).
- Rozkład trudności: 6 Easy / 9 Medium / 5 Hard.
- Pytania "select ALL"/"pick TWO" ze źródła przerobione na single-correct (NOT-question / "który prawdziwy/fałszywy") — wymóg invariantu Domain `MultipleChoice` (dokładnie jedna poprawna opcja).
- `DataSeeder` — druga `SeedCategoryIfMissingAsync` (name "SQL", iconCode "SQL"). Bez zmian schematu, bez migracji; seeder idempotentny per-nazwa, więc kategoria dosypie się przy restarcie API bez dropowania bazy.
- `DataSeederTests` — zaktualizowane liczniki: 2 kategorie, 39 pytań (19+20), 156 opcji (39×4); `SingleAsync()` na nazwie → `ToListAsync()` + `BeEquivalentTo(["Unit Testing","SQL"])`.

**Decyzje:**
- `iconCode = "SQL"` (3 znaki) — badge w gridzie renderuje `iconCode` jako tekst w boxie 32px; krótki kod pasuje (istniejące "test-tube" się nie mieści, ale to pre-existing, nie ruszam).

**Weryfikacja:**
- `dotnet build` Infrastructure + testy — zielone.
- Invariant ręcznie potwierdzony na pliku: 20× `Question.Create`, 20× `isCorrect: true` (jedna na pytanie), 80× `new Option` (4 na pytanie).
- **Testy integracyjne (Testcontainers) NIE odpalone lokalnie** — Docker Desktop był ubity w trakcie sesji (named-pipe niedostępny). Bramką jest CI (ma Dockera). Wizualne potwierdzenie po stronie ownera po wstaniu stacku.

**Punkt wznowienia:**
PR SQL → po zielonym CI merge (za potwierdzeniem ownera). Następne w batchu 1: EF Core, ASP.NET Core, C#/.NET.

---

## 2026-06-08 — Content expansion batch 1: EF Core question bank (kategoria #3)

**Co zrobione (branch `feat/efcore-question-bank`, issue #134):**
- `EfCoreQuestions.cs` — bank 20 pytań MultipleChoice, źródło: kurs EPAM moduł 013 (Entity Framework Core) graded + ungraded quizy, uzupełnione pod interview. Zakres: rola ORM, ADO.NET vs EF Core, DbContext/DbSet, code-first/database-first, migracje (add/update), data annotations vs Fluent API + precedencja, strategie ładowania (eager/lazy/explicit), Include, change tracking, AsNoTracking, optimistic concurrency, Remove→DELETE.
- Rozkład trudności: 6 Easy / 9 Medium / 5 Hard.
- Pytania "select ALL"/"pick TWO" przerobione na single-correct (invariant `MultipleChoice`).
- `DataSeeder` — trzecia `SeedCategoryIfMissingAsync` (name "EF Core", iconCode "EF").
- `DataSeederTests` — liczniki: 3 kategorie, 59 pytań (19+20+20), 236 opcji (59×4), nazwy `["Unit Testing","SQL","EF Core"]`.

**Weryfikacja:**
- `dotnet build` testów — zielone; invariant ręcznie: 20× `Question.Create`, 20× `isCorrect: true`, 80× `new Option`.
- Testy integracyjne (Testcontainers) — bramką CI (Docker lokalnie nadal ubity).

**Punkt wznowienia:** PR EF Core → po zielonym CI merge (za potwierdzeniem). Następne w batchu 1: ASP.NET Core, potem C#/.NET.
