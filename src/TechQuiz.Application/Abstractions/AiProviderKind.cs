namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Identifies an AI generation backend (ADR-006, ADR-019). <see cref="Anthropic"/>,
/// <see cref="OpenAi"/>, and <see cref="Gemini"/> are native, bring-your-own-key
/// providers so users use the key they already hold; <see cref="OpenRouter"/> is an
/// additional one-key-many-models option. Anthropic is implemented first (the only
/// kind verifiable live today); the others get their own iterations — until then the
/// resolver throws <c>UnknownAiProviderException</c> for an unregistered kind.
/// </summary>
public enum AiProviderKind
{
    Anthropic,
    OpenAi,
    Gemini,
    OpenRouter
}
