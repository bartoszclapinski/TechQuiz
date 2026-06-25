namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Identifies an AI generation backend (ADR-006). Native support ships for
/// <see cref="Anthropic"/> only; <see cref="OpenRouter"/> is the planned
/// multi-model path (one OpenAI-compatible key for GPT / Gemini / open models)
/// and arrives behind a later ADR amending ADR-006.
/// </summary>
public enum AiProviderKind
{
    Anthropic,
    OpenRouter
}
