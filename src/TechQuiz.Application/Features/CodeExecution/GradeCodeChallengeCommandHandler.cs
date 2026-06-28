using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Loads a challenge from the catalog, then grades a C# submission in two stages: the
/// first sandbox run doubles as a compile gate, and the hidden test cases are only
/// evaluated once the submission compiles. On a compilation error the grade short-circuits
/// — the compiler output is surfaced and no further cases are run. Otherwise each case
/// feeds its own stdin and observed stdout is compared via
/// <see cref="Domain.CodeChallengeTestCase.Matches"/>; the challenge passes only when
/// every case passes.
/// </summary>
public sealed class GradeCodeChallengeCommandHandler(
    ICodeChallengeCatalog catalog,
    ICodeExecutor codeExecutor)
    : IRequestHandler<GradeCodeChallengeCommand, CodeChallengeGradeResult>
{
    // Judge0's canonical description for status id 6. Compilation does not depend on stdin,
    // so the first run's verdict applies to the whole submission.
    private const string CompilationErrorStatus = "Compilation Error";

    public async Task<CodeChallengeGradeResult> Handle(
        GradeCodeChallengeCommand request,
        CancellationToken cancellationToken)
    {
        var challenge = catalog.Find(request.ChallengeId)
            ?? throw new KeyNotFoundException(
                $"CodeChallenge '{request.ChallengeId}' was not found.");

        var orderedCases = challenge.TestCases.OrderBy(tc => tc.OrderIndex).ToList();
        var cases = new List<CodeChallengeCaseResult>(orderedCases.Count);

        foreach (var testCase in orderedCases)
        {
            var execution = await codeExecutor.ExecuteCSharpAsync(
                new CodeExecutionRequest(request.SourceCode, testCase.Stdin),
                cancellationToken);

            if (execution.Status == CompilationErrorStatus)
            {
                return new CodeChallengeGradeResult(
                    Compiled: false,
                    CompileOutput: execution.CompileOutput,
                    Passed: false,
                    PassedCount: 0,
                    TotalCount: orderedCases.Count,
                    Cases: []);
            }

            cases.Add(new CodeChallengeCaseResult(
                OrderIndex: testCase.OrderIndex,
                Passed: testCase.Matches(execution.Stdout),
                Status: execution.Status,
                ActualStdout: execution.Stdout,
                Stderr: execution.Stderr,
                CompileOutput: execution.CompileOutput));
        }

        var passedCount = cases.Count(c => c.Passed);

        return new CodeChallengeGradeResult(
            Compiled: true,
            CompileOutput: null,
            Passed: passedCount == cases.Count,
            PassedCount: passedCount,
            TotalCount: cases.Count,
            Cases: cases);
    }
}
