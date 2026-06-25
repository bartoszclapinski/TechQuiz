using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Features.Pool;

namespace TechQuiz.Api.Controllers;

/// <summary>
/// The public pool of AI-generated questions (ADR-007, ADR-020). Browsing is open to any
/// authenticated user and never exposes the correct option (hard rule #4). Publishing promotes
/// the caller's own draft into the pool; the handler enforces ownership (403) and rejects an
/// already-published draft (409).
/// </summary>
[ApiController]
[Route("api/pool/questions")]
[Authorize]
public sealed class PoolController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists the published pool questions, newest first. No answer key.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PooledQuestionResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var questions = await mediator.Send(new ListPooledQuestionsQuery(), cancellationToken);

        // Difficulty crosses the wire as its enum name, matching the generate preview; the quiz
        // endpoints' numeric contract is why there's no global string-enum converter to lean on.
        var response = questions
            .Select(q => new PooledQuestionResponse(
                q.Id,
                q.Stem,
                q.Options,
                q.Difficulty.ToString(),
                q.Explanation,
                q.Provider,
                q.Topic,
                q.CreatedByUserId,
                q.GeneratedAtUtc))
            .ToArray();

        return Ok(response);
    }

    /// <summary>
    /// Publishes the caller's own draft. 404 if the draft is unknown, 403 if it belongs to
    /// another user, 409 if it is already published.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new PublishPooledQuestionCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record PooledQuestionResponse(
        Guid Id,
        string Stem,
        IReadOnlyList<string> Options,
        string Difficulty,
        string? Explanation,
        string Provider,
        string Topic,
        Guid CreatedByUserId,
        DateTime GeneratedAtUtc);
}
