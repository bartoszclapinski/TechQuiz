using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.CodeExecution;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/code")]
[Authorize]
public sealed class CodeExecutionController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Spike endpoint (ADR-018): compile + run a C# snippet in the Judge0 sandbox and
    /// return its verdict. No hidden-test grading yet — that lands in a later iteration.
    /// </summary>
    [HttpPost("run")]
    public async Task<ActionResult<CodeExecutionResult>> Run(
        [FromBody] RunCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RunCodeCommand(request.SourceCode, request.Stdin), cancellationToken);
        return Ok(result);
    }

    public sealed record RunCodeRequest(string SourceCode, string? Stdin);
}
