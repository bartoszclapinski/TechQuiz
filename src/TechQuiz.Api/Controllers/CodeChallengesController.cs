using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Features.CodeExecution;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/code-challenges")]
[Authorize]
public sealed class CodeChallengesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lists the available coding challenges (ADR-018). The hidden test cases are never
    /// included — only prompt and starter code are exposed.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CodeChallengeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var challenges = await mediator.Send(new GetCodeChallengesQuery(), cancellationToken);
        return Ok(challenges);
    }

    /// <summary>
    /// Compiles and runs a C# submission against the challenge's hidden test cases in the
    /// Judge0 sandbox and returns a per-case verdict. Unknown challenge id yields 404.
    /// </summary>
    [HttpPost("{id:guid}/grade")]
    public async Task<ActionResult<CodeChallengeGradeResult>> Grade(
        Guid id,
        [FromBody] GradeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GradeCodeChallengeCommand(id, request.SourceCode), cancellationToken);
        return Ok(result);
    }

    public sealed record GradeRequest(string SourceCode);
}
