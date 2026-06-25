namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Strongly-typed binding for the <c>Ai:Anthropic:*</c> configuration section. Only
/// non-secret settings live here — the API key is per-user and supplied per call
/// (bring-your-own-key, ADR-006), never read from configuration.
/// </summary>
public sealed class AnthropicOptions
{
    public const string SectionName = "Ai:Anthropic";

    public string BaseUrl { get; set; } = "https://api.anthropic.com/";

    /// <summary>Messages-API model id. A cheap, fast model is the sensible default for quiz drafts.</summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";

    /// <summary>The <c>anthropic-version</c> header value.</summary>
    public string ApiVersion { get; set; } = "2023-06-01";

    public int MaxTokens { get; set; } = 4096;
}
