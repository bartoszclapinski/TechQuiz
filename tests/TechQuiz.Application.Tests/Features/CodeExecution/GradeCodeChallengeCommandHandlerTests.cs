using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.CodeExecution;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.CodeExecution;

public class GradeCodeChallengeCommandHandlerTests
{
    private readonly ICodeChallengeCatalog _catalog = Substitute.For<ICodeChallengeCatalog>();
    private readonly ICodeExecutor _codeExecutor = Substitute.For<ICodeExecutor>();

    private GradeCodeChallengeCommandHandler CreateSut() => new(_catalog, _codeExecutor);

    private CodeChallenge SeedChallengeWithTwoCases()
    {
        var challenge = CodeChallenge.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Difficulty.Easy,
            title: "Sum two integers",
            prompt: "Print the sum of two space-separated integers.",
            starterCode: null,
            testCases:
            [
                new CodeChallengeTestCase(stdin: "2 3", expectedStdout: "5", orderIndex: 0),
                new CodeChallengeTestCase(stdin: "10 20", expectedStdout: "30", orderIndex: 1),
            ]);
        _catalog.Find(challenge.Id).Returns(challenge);
        return challenge;
    }

    private void ExecutorReturnsStdout(string stdin, string? stdout, string status = "Accepted") =>
        _codeExecutor
            .ExecuteCSharpAsync(
                Arg.Is<CodeExecutionRequest>(r => r.Stdin == stdin),
                Arg.Any<CancellationToken>())
            .Returns(new CodeExecutionResult(status, stdout, null, null, 0.05, 4096));

    [Fact]
    public async Task Handle_AllTestCasesPass_ReturnsPassed()
    {
        var challenge = SeedChallengeWithTwoCases();
        ExecutorReturnsStdout("2 3", "5\n");
        ExecutorReturnsStdout("10 20", "30\n");

        var result = await CreateSut().Handle(
            new GradeCodeChallengeCommand(challenge.Id, "// source"), CancellationToken.None);

        result.Compiled.Should().BeTrue();
        result.Passed.Should().BeTrue();
        result.PassedCount.Should().Be(2);
        result.TotalCount.Should().Be(2);
        result.Cases.Should().OnlyContain(c => c.Passed);
    }

    [Fact]
    public async Task Handle_OneTestCaseFails_ReturnsNotPassed()
    {
        var challenge = SeedChallengeWithTwoCases();
        ExecutorReturnsStdout("2 3", "5\n");
        ExecutorReturnsStdout("10 20", "31\n");

        var result = await CreateSut().Handle(
            new GradeCodeChallengeCommand(challenge.Id, "// source"), CancellationToken.None);

        result.Passed.Should().BeFalse();
        result.PassedCount.Should().Be(1);
        result.TotalCount.Should().Be(2);
        result.Cases.Single(c => c.OrderIndex == 1).Passed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RunsExecutorOncePerTestCase_WithEachStdin()
    {
        var challenge = SeedChallengeWithTwoCases();
        ExecutorReturnsStdout("2 3", "5");
        ExecutorReturnsStdout("10 20", "30");

        await CreateSut().Handle(
            new GradeCodeChallengeCommand(challenge.Id, "// source"), CancellationToken.None);

        await _codeExecutor.Received(1).ExecuteCSharpAsync(
            Arg.Is<CodeExecutionRequest>(r => r.SourceCode == "// source" && r.Stdin == "2 3"),
            Arg.Any<CancellationToken>());
        await _codeExecutor.Received(1).ExecuteCSharpAsync(
            Arg.Is<CodeExecutionRequest>(r => r.SourceCode == "// source" && r.Stdin == "10 20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CompilationFails_ShortCircuits_WithoutEvaluatingCases()
    {
        var challenge = SeedChallengeWithTwoCases();
        _codeExecutor
            .ExecuteCSharpAsync(Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CodeExecutionResult(
                Status: "Compilation Error",
                Stdout: null,
                Stderr: null,
                CompileOutput: "error CS1002: ; expected",
                TimeSeconds: null,
                MemoryKb: null));

        var result = await CreateSut().Handle(
            new GradeCodeChallengeCommand(challenge.Id, "broken"), CancellationToken.None);

        result.Compiled.Should().BeFalse();
        result.CompileOutput.Should().Be("error CS1002: ; expected");
        result.Passed.Should().BeFalse();
        result.PassedCount.Should().Be(0);
        result.TotalCount.Should().Be(2);
        result.Cases.Should().BeEmpty();

        // The compile gate runs once; a non-compiling submission is not replayed per case.
        await _codeExecutor.Received(1).ExecuteCSharpAsync(
            Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChallengeNotFound_Throws()
    {
        var missingId = Guid.NewGuid();
        _catalog.Find(missingId).Returns((CodeChallenge?)null);

        var act = () => CreateSut().Handle(
            new GradeCodeChallengeCommand(missingId, "// source"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{missingId}*");

        await _codeExecutor.DidNotReceive().ExecuteCSharpAsync(
            Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>());
    }
}
