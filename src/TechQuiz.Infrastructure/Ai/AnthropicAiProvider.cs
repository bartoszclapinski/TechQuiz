using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
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
        await ThrowIfRejectedAsync(response, cancellationToken);
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

    /// <summary>
    /// Turns the key/quota failures a user can act on into <see cref="AiProviderRejectedException"/>.
    /// Anthropic reports an exhausted credit balance as a 400 <c>invalid_request_error</c>, so that
    /// case is recognised by its message. The upstream body is only inspected, never passed on.
    /// Any other failure falls through to <c>EnsureSuccessStatusCode</c>.
    /// </summary>
    private static async Task ThrowIfRejectedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var outOfCredit = response.StatusCode == HttpStatusCode.BadRequest
            && (await response.Content.ReadAsStringAsync(cancellationToken))
                .Contains("credit balance", StringComparison.OrdinalIgnoreCase);

        AiRejectionReason? reason = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => AiRejectionReason.InvalidKey,
            HttpStatusCode.TooManyRequests => AiRejectionReason.QuotaOrRateLimit,
            _ when outOfCredit => AiRejectionReason.QuotaOrRateLimit,
            _ => null,
        };

        if (reason is { } rejected)
        {
            throw new AiProviderRejectedException(AiProviderKind.Anthropic, rejected);
        }
    }

    private sealed record AnthropicMessageResponse(
        [property: JsonPropertyName("content")] IReadOnlyList<AnthropicContentBlock>? Content);

    private sealed record AnthropicContentBlock(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string? Text);
}
