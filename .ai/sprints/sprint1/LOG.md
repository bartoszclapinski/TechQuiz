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
