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
