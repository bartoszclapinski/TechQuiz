namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Strongly-typed binding for the <c>Ai:OpenAi:*</c> configuration section. Only non-secret
/// settings live here — the API key is per-user and supplied per call (bring-your-own-key,
/// ADR-006), never read from configuration.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "Ai:OpenAi";

    public string BaseUrl { get; set; } = "https://api.openai.com/";

    /// <summary>Chat Completions model id. The fastest, cheapest GPT-6 tier suits quiz drafts.</summary>
    public string Model { get; set; } = "gpt-6-luna";

    /// <summary>
    /// Upper bound on generated tokens, reasoning tokens included — so it is set well above
    /// what the visible answer needs.
    /// </summary>
    public int MaxCompletionTokens { get; set; } = 8192;

    /// <summary><c>reasoning_effort</c>: <c>low</c> keeps drafts fast and cheap.</summary>
    public string ReasoningEffort { get; set; } = "low";
}
