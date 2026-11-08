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
/// the provider is a stateless singleton. The model is prompted for a JSON object (see
/// <see cref="AiPrompts"/>), which is parsed into <see cref="GeneratedQuestionDraft"/>s.
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
        var text = await RequestTextAsync(AiPrompts.BuildQuestionsPrompt(request), apiKey, cancellationToken);
        return AiPrompts.ParseDrafts(text, request.Difficulty, "Anthropic");
    }

    public Task<string> GenerateCodeFeedbackAsync(
        CodeFeedbackRequest request,
        string apiKey,
        CancellationToken cancellationToken = default) =>
        RequestTextAsync(AiPrompts.BuildFeedbackPrompt(request), apiKey, cancellationToken);

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

    private sealed record AnthropicMessageResponse(
        [property: JsonPropertyName("content")] IReadOnlyList<AnthropicContentBlock>? Content);

    private sealed record AnthropicContentBlock(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string? Text);
}
