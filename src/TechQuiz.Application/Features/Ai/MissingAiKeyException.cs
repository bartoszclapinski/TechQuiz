using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Thrown when generation is requested for a provider the current user has not
/// configured a key for. Surfaced as a 4xx (the user must add a key), never an
/// unhandled 500.
/// </summary>
public sealed class MissingAiKeyException(AiProviderKind kind)
    : Exception($"No API key is configured for provider '{kind}'.")
{
    public AiProviderKind Kind { get; } = kind;
}
