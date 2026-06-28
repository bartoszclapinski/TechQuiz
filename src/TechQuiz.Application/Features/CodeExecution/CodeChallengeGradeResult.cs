namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Outcome of grading a C# submission against a CodeChallenge's hidden test cases.
/// Grading is two-stage: the submission is compiled first, and test cases are only
/// evaluated when it compiles. <see cref="Compiled"/> is false when compilation failed,
/// in which case <see cref="CompileOutput"/> carries the compiler diagnostics and
/// <see cref="Cases"/> is empty. <see cref="Passed"/> is true only when every case passed.
/// </summary>
public sealed record CodeChallengeGradeResult(
    bool Compiled,
    string? CompileOutput,
    bool Passed,
    int PassedCount,
    int TotalCount,
    IReadOnlyList<CodeChallengeCaseResult> Cases);

public sealed record CodeChallengeCaseResult(
    int OrderIndex,
    bool Passed,
    string Status,
    string? ActualStdout,
    string? Stderr,
    string? CompileOutput);
