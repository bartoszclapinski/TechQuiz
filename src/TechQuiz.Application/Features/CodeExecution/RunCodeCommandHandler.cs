using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed class RunCodeCommandHandler(ICodeExecutor codeExecutor)
    : IRequestHandler<RunCodeCommand, CodeExecutionResult>
{
    public Task<CodeExecutionResult> Handle(
        RunCodeCommand request,
        CancellationToken cancellationToken)
    {
        var executionRequest = new CodeExecutionRequest(request.SourceCode, request.Stdin);
        return codeExecutor.ExecuteCSharpAsync(executionRequest, cancellationToken);
    }
}
