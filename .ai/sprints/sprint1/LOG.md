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
