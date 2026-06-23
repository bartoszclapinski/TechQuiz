using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.CodeExecution;

namespace TechQuiz.Application.Tests.Features.CodeExecution;

public class RunCodeCommandHandlerTests
{
    private readonly ICodeExecutor _codeExecutor = Substitute.For<ICodeExecutor>();

    private RunCodeCommandHandler CreateSut() => new(_codeExecutor);

    [Fact]
    public async Task Handle_MapsCommandToRequest_AndReturnsExecutorResult()
    {
        var expected = new CodeExecutionResult(
            Status: "Accepted",
            Stdout: "42\n",
            Stderr: null,
            CompileOutput: null,
            TimeSeconds: 0.12,
            MemoryKb: 8192);
        _codeExecutor
            .ExecuteCSharpAsync(Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await CreateSut().Handle(
            new RunCodeCommand("Console.WriteLine(42);", "stdin-data"), CancellationToken.None);

        result.Should().BeSameAs(expected);
        await _codeExecutor.Received(1).ExecuteCSharpAsync(
            Arg.Is<CodeExecutionRequest>(r =>
                r.SourceCode == "Console.WriteLine(42);" && r.Stdin == "stdin-data"),
            Arg.Any<CancellationToken>());
    }
}
