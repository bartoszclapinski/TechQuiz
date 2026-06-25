using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Thrown when generation is requested for an <see cref="AiProviderKind"/> that
/// has no registered <see cref="IAiProvider"/> implementation (e.g. the provider
/// is planned but not yet wired, or not configured in this environment).
/// </summary>
public sealed class UnknownAiProviderException(AiProviderKind kind)
    : Exception($"No AI provider is registered for '{kind}'.")
{
    public AiProviderKind Kind { get; } = kind;
}
