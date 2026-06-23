using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Loads a challenge from the catalog, then grades a C# submission by running it through
/// the sandbox once per hidden test case (each case feeds its own stdin) and comparing
/// observed stdout via <see cref="Domain.CodeChallengeTestCase.Matches"/>. The challenge
/// passes only when every case passes.
/// </summary>
public sealed class GradeCodeChallengeCommandHandler(
    ICodeChallengeCatalog catalog,
    ICodeExecutor codeExecutor)
    : IRequestHandler<GradeCodeChallengeCommand, CodeChallengeGradeResult>
{
    public async Task<CodeChallengeGradeResult> Handle(
        GradeCodeChallengeCommand request,
        CancellationToken cancellationToken)
    {
        var challenge = catalog.Find(request.ChallengeId)
            ?? throw new KeyNotFoundException(
                $"CodeChallenge '{request.ChallengeId}' was not found.");

        var cases = new List<CodeChallengeCaseResult>(challenge.TestCases.Count);

        foreach (var testCase in challenge.TestCases.OrderBy(tc => tc.OrderIndex))
        {
            var execution = await codeExecutor.ExecuteCSharpAsync(
                new CodeExecutionRequest(request.SourceCode, testCase.Stdin),
                cancellationToken);

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
            Passed: passedCount == cases.Count,
            PassedCount: passedCount,
            TotalCount: cases.Count,
            Cases: cases);
    }
}
