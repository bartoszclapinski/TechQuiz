namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Outcome of grading a C# submission against a CodeChallenge's hidden test cases.
/// <see cref="Passed"/> is true only when every case passed.
/// </summary>
public sealed record CodeChallengeGradeResult(
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
