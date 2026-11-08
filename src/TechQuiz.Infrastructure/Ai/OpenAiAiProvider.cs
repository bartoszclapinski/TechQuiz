using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Generates quiz drafts and code feedback via the OpenAI Chat Completions API using the
/// current user's own API key (bring-your-own-key, ADR-006, ADR-019). The key is attached as
/// a Bearer token on each request only — never stored on the client — so the provider is a
/// stateless singleton. Question generation uses strict <c>json_schema</c> output, so the reply
/// is already in the shared <c>{ "questions": [...] }</c> shape that <see cref="AiPrompts"/>
/// parses. Every request sends <c>store: false</c>: user prompts and code are not retained
/// for OpenAI's dashboard.
/// </summary>
internal sealed class OpenAiAiProvider(HttpClient http, IOptions<OpenAiOptions> options)
    : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public AiProviderKind Kind => AiProviderKind.OpenAi;

    public async Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
        GenerateQuestionsRequest request,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var text = await RequestTextAsync(
            AiPrompts.BuildQuestionsPrompt(request), QuestionsResponseFormat, apiKey, cancellationToken);
        return AiPrompts.ParseDrafts(text, request.Difficulty, "OpenAI");
    }

    public Task<string> GenerateCodeFeedbackAsync(
        CodeFeedbackRequest request,
        string apiKey,
        CancellationToken cancellationToken = default) =>
        RequestTextAsync(AiPrompts.BuildFeedbackPrompt(request), responseFormat: null, apiKey, cancellationToken);

    private async Task<string> RequestTextAsync(
        string prompt, object? responseFormat, string apiKey, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        var payload = new ChatCompletionRequest(
            Model: opts.Model,
            Messages: [new ChatMessage("user", prompt)],
            MaxCompletionTokens: opts.MaxCompletionTokens,
            ReasoningEffort: opts.ReasoningEffort,
            Store: false,
            ResponseFormat: responseFormat);

        using var message = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = JsonContent.Create(payload, options: RequestJsonOptions),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await http.SendAsync(message, cancellationToken);
        ThrowIfRejected(response);
        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken)
            ?? throw new AiResponseException("OpenAI returned an empty response body.");

        var choice = body.Choices?.FirstOrDefault()
            ?? throw new AiResponseException("OpenAI response contained no choices.");

        if (!string.IsNullOrWhiteSpace(choice.Message?.Refusal))
        {
            throw new AiResponseException("OpenAI declined to answer this request.");
        }

        if (choice.FinishReason == "length")
        {
            throw new AiResponseException("OpenAI stopped at the token limit before finishing the answer.");
        }

        var text = choice.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new AiResponseException("OpenAI response contained no text content.");
        }

        return text;
    }

    /// <summary>
    /// Turns the key/quota failures a user can act on into <see cref="AiProviderRejectedException"/>.
    /// OpenAI reports both rate limits and an exhausted balance (<c>insufficient_quota</c>) as 429.
    /// The upstream body is never passed on. Any other failure falls through to
    /// <c>EnsureSuccessStatusCode</c>.
    /// </summary>
    private static void ThrowIfRejected(HttpResponseMessage response)
    {
        AiRejectionReason? reason = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => AiRejectionReason.InvalidKey,
            HttpStatusCode.TooManyRequests => AiRejectionReason.QuotaOrRateLimit,
            _ => null,
        };

        if (reason is { } rejected)
        {
            throw new AiProviderRejectedException(AiProviderKind.OpenAi, rejected);
        }
    }

    private static readonly JsonSerializerOptions RequestJsonOptions =
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    // Strict structured output: every property required, no extras, exactly four options.
    // "explanation" is nullable rather than optional because strict mode requires every key.
    private static readonly object QuestionsResponseFormat = new
    {
        type = "json_schema",
        json_schema = new
        {
            name = "quiz_questions",
            strict = true,
            schema = new
            {
                type = "object",
                additionalProperties = false,
                required = new[] { "questions" },
                properties = new
                {
                    questions = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            additionalProperties = false,
                            required = new[] { "stem", "options", "correctOptionIndex", "explanation" },
                            properties = new
                            {
                                stem = new { type = "string" },
                                options = new
                                {
                                    type = "array",
                                    items = new { type = "string" },
                                    minItems = 4,
                                    maxItems = 4,
                                },
                                correctOptionIndex = new { type = "integer", minimum = 0, maximum = 3 },
                                explanation = new { type = new[] { "string", "null" } },
                            },
                        },
                    },
                },
            },
        },
    };

    private sealed record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("max_completion_tokens")] int MaxCompletionTokens,
        [property: JsonPropertyName("reasoning_effort")] string ReasoningEffort,
        [property: JsonPropertyName("store")] bool Store,
        [property: JsonPropertyName("response_format")] object? ResponseFormat);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice>? Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatResponseMessage? Message,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);

    private sealed record ChatResponseMessage(
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("refusal")] string? Refusal);
}
