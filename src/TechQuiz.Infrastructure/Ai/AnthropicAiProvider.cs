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
        var text = await RequestTextAsync(BuildPrompt(request), apiKey, cancellationToken);
        return ParseDrafts(text, request.Difficulty);
    }

    public Task<string> GenerateCodeFeedbackAsync(
        CodeFeedbackRequest request,
        string apiKey,
        CancellationToken cancellationToken = default) =>
        RequestTextAsync(BuildFeedbackPrompt(request), apiKey, cancellationToken);

    /// <summary>
    /// POSTs a single user message to the Messages API with the caller's key and returns the
    /// model's text block. Shared by question generation and code feedback; callers parse or
    /// display the returned text as needed.
    /// </summary>
    private async Task<string> RequestTextAsync(
        string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        var payload = new
        {
            model = opts.Model,
            max_tokens = opts.MaxTokens,
            messages = new[] { new { role = "user", content = prompt } },
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

        return text;
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

    private static string BuildFeedbackPrompt(CodeFeedbackRequest request)
    {
        // The hidden cases are given so the model can reason about edge cases the submission
        // misses, but it must NOT quote the exact expected outputs back — that would leak the
        // harness (spirit of hard rule #4). Guidance is qualitative: "you don't handle empty
        // input", not "test 2 expects 30".
        var cases = string.Join(
            "\n",
            request.TestCases.Select((c, i) =>
                $"  Case {i + 1}: stdin={Quote(c.Stdin)} expectedStdout={Quote(c.ExpectedStdout)}"));

        return $$"""
        You are a senior C# reviewer giving feedback to a developer on a coding exercise.

        Challenge: {{request.ChallengeTitle}}
        Task: {{request.Prompt}}

        The developer's submission:
        ```csharp
        {{request.SourceCode}}
        ```

        Hidden test cases (for your reasoning only):
        {{cases}}

        Write concise, constructive feedback in markdown prose: what the code does well, what is
        risky or incorrect, and which edge cases it may miss. Be specific about the code, but do
        NOT quote or restate the exact expected outputs above — describe the behaviour the code
        should have ("handle empty input", "trim trailing whitespace") rather than the literal
        expected values. Do not include a numeric score; the automated tests already decide pass
        or fail. Keep it under ~200 words.
        """;
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value);

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
