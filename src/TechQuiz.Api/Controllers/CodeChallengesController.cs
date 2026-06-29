using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Abstractions;
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

    /// <summary>
    /// Returns on-demand qualitative AI feedback on a submission via the caller's bring-your-own-key
    /// provider (ADR-018). Complementary to <see cref="Grade"/> — it never decides pass/fail. Unknown
    /// challenge yields 404; no stored key for the provider yields 409. The provider crosses the wire
    /// as its enum name, mapped here (the same convention as the generate endpoint).
    /// </summary>
    [HttpPost("{id:guid}/feedback")]
    public async Task<ActionResult<FeedbackResponse>> Feedback(
        Guid id,
        [FromBody] FeedbackRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiProviderKind>(request.Provider, ignoreCase: true, out var provider)
            || !Enum.IsDefined(provider))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["provider"] = [$"'{request.Provider}' is not a known provider."],
            }));
        }

        var result = await mediator.Send(
            new GetCodeFeedbackCommand(id, request.SourceCode, provider), cancellationToken);

        return Ok(new FeedbackResponse(result.Feedback, result.Provider.ToString()));
    }

    public sealed record GradeRequest(string SourceCode);

    public sealed record FeedbackRequest(string SourceCode, string Provider);

    public sealed record FeedbackResponse(string Feedback, string Provider);
}
