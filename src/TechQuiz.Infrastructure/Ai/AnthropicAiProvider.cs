using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Generates quiz drafts via the Anthropic Messages API using the current user's own
/// API key (bring-your-own-key, ADR-006). The key is supplied per call and attached as
/// the <c>x-api-key</c> header on that request only — never stored on the client — so
/// the provider is a stateless singleton. The model is prompted for a strict JSON array,
/// which is parsed into <see cref="GeneratedQuestionDraft"/>s at the requested difficulty.
/// </summary>
internal sealed class AnthropicAiProvider(HttpClient http, IOptions<AnthropicOptions> options)
    : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public AiProviderKind Kind => AiProviderKind.Anthropic;

    public async Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
        GenerateQuestionsRequest request,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;

        var payload = new
        {
            model = opts.Model,
            max_tokens = opts.MaxTokens,
            messages = new[] { new { role = "user", content = BuildPrompt(request) } },
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = JsonContent.Create(payload),
        };
        message.Headers.Add("x-api-key", apiKey);
        message.Headers.Add("anthropic-version", opts.ApiVersion);

        using var response = await http.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<AnthropicMessageResponse>(JsonOptions, cancellationToken)
            ?? throw new AiResponseException("Anthropic returned an empty response body.");

        var text = body.Content?.FirstOrDefault(b => b.Type == "text")?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new AiResponseException("Anthropic response contained no text content.");
        }

        return ParseDrafts(text, request.Difficulty);
    }

    private static string BuildPrompt(GenerateQuestionsRequest request) =>
        $$"""
        Generate {{request.Count}} multiple-choice quiz question(s) about "{{request.Topic}}"
        at {{request.Difficulty}} difficulty for a developer self-assessment.

        Respond with ONLY a JSON array (no prose, no markdown fences). Each element:
        {
          "stem": "the question text",
          "options": ["four", "distinct", "answer", "choices"],
          "correctOptionIndex": 0,
          "explanation": "one sentence explaining the correct answer"
        }

        Exactly four options per question. correctOptionIndex is the 0-based index of the
        correct option.
        """;

    private static IReadOnlyList<GeneratedQuestionDraft> ParseDrafts(string text, Difficulty difficulty)
    {
        var json = StripJsonFences(text);

        DraftDto[]? drafts;
        try
        {
            drafts = JsonSerializer.Deserialize<DraftDto[]>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new AiResponseException($"Anthropic response was not valid question JSON: {ex.Message}");
        }

        if (drafts is null || drafts.Length == 0)
        {
            throw new AiResponseException("Anthropic response contained no questions.");
        }

        return drafts
            .Select(d => new GeneratedQuestionDraft(
                d.Stem, d.Options, d.CorrectOptionIndex, difficulty, d.Explanation))
            .ToArray();
    }

    // Models sometimes wrap JSON in ```json … ``` despite instructions; tolerate it.
    private static string StripJsonFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        var withoutOpen = firstNewline >= 0 ? trimmed[(firstNewline + 1)..] : trimmed;
        var closingFence = withoutOpen.LastIndexOf("```", StringComparison.Ordinal);
        return (closingFence >= 0 ? withoutOpen[..closingFence] : withoutOpen).Trim();
    }

    private sealed record DraftDto(
        string Stem,
        IReadOnlyList<string> Options,
        int CorrectOptionIndex,
        string? Explanation);

    private sealed record AnthropicMessageResponse(
        [property: JsonPropertyName("content")] IReadOnlyList<AnthropicContentBlock>? Content);

    private sealed record AnthropicContentBlock(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string? Text);
}
