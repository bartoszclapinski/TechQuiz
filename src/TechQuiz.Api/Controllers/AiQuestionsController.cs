using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;

namespace TechQuiz.Api.Controllers;

/// <summary>
/// Generates draft quiz questions from a topic via the current user's bring-your-own-key
/// provider (ADR-006). Difficulty and provider cross the wire as their enum names, mapped
/// here rather than via a global string-enum converter, which the quiz endpoints' numeric
/// contract depends on not having. Correct-answer indices are never serialized (hard rule #4).
/// </summary>
[ApiController]
[Route("api/ai/questions")]
[Authorize]
public sealed class AiQuestionsController(IMediator mediator) : ControllerBase
{
    /// <summary>Generates drafts for a topic. Requires a stored key for the chosen provider (else 409).</summary>
    [HttpPost]
    public async Task<ActionResult<GenerateQuestionsResponse>> Generate(
        [FromBody] GenerateQuestionsApiRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Difficulty>(request.Difficulty, ignoreCase: true, out var difficulty)
            || !Enum.IsDefined(difficulty))
        {
            return Unknown("difficulty", request.Difficulty);
        }

        if (!Enum.TryParse<AiProviderKind>(request.Provider, ignoreCase: true, out var provider)
            || !Enum.IsDefined(provider))
        {
            return Unknown("provider", request.Provider);
        }

        var result = await mediator.Send(
            new GenerateQuestionsCommand(request.Topic, difficulty, request.Count, provider),
            cancellationToken);

        var questions = result.Questions
            .Select(q => new GeneratedDraftDto(
                q.Id, q.Stem, q.Options, q.Difficulty.ToString(), q.Explanation))
            .ToArray();

        return Ok(new GenerateQuestionsResponse(result.Provider.ToString(), questions));
    }

    private ActionResult Unknown(string field, string value) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [field] = [$"'{value}' is not a known {field}."],
        }));

    public sealed record GenerateQuestionsApiRequest(
        string Topic, string Difficulty, int Count, string Provider);

    public sealed record GenerateQuestionsResponse(
        string Provider, IReadOnlyList<GeneratedDraftDto> Questions);

    public sealed record GeneratedDraftDto(
        Guid Id, string Stem, IReadOnlyList<string> Options, string Difficulty, string? Explanation);
}
