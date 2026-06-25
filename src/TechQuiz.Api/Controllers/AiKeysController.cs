using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Api.Controllers;

/// <summary>
/// Manages the current user's bring-your-own-key AI credentials (ADR-006). Providers are
/// exchanged as their enum names (e.g. "Anthropic") so the contract stays readable without
/// a global string-enum converter, which the quiz endpoints' numeric contract depends on
/// not having. Key material is only ever accepted, never returned.
/// </summary>
[ApiController]
[Route("api/ai/keys")]
[Authorize]
public sealed class AiKeysController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists the providers the current user has configured — kinds only, never the key.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> GetConfigured(CancellationToken cancellationToken)
    {
        var kinds = await mediator.Send(new GetConfiguredProvidersQuery(), cancellationToken);
        return Ok(kinds.Select(k => k.ToString()).ToArray());
    }

    /// <summary>Stores or rotates the current user's key for a provider.</summary>
    [HttpPut]
    public async Task<IActionResult> Set(
        [FromBody] SetAiKeyRequest request, CancellationToken cancellationToken)
    {
        if (!TryParseProvider(request.Provider, out var provider))
        {
            return UnknownProvider(request.Provider);
        }

        await mediator.Send(new SetAiKeyCommand(provider, request.ApiKey), cancellationToken);
        return NoContent();
    }

    /// <summary>Removes the current user's key for a provider. Idempotent.</summary>
    [HttpDelete("{provider}")]
    public async Task<IActionResult> Remove(string provider, CancellationToken cancellationToken)
    {
        if (!TryParseProvider(provider, out var kind))
        {
            return UnknownProvider(provider);
        }

        await mediator.Send(new RemoveAiKeyCommand(kind), cancellationToken);
        return NoContent();
    }

    private static bool TryParseProvider(string value, out AiProviderKind provider) =>
        Enum.TryParse(value, ignoreCase: true, out provider) && Enum.IsDefined(provider);

    private ActionResult UnknownProvider(string value) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["provider"] = [$"'{value}' is not a known AI provider."],
        }));

    public sealed record SetAiKeyRequest(string Provider, string ApiKey);
}
