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
