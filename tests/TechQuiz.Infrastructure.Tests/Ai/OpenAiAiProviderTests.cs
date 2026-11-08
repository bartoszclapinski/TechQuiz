using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Ai;

namespace TechQuiz.Infrastructure.Tests.Ai;

public sealed class OpenAiAiProviderTests
{
    private readonly CapturingHandler _handler = new();

    private OpenAiAiProvider CreateSut()
    {
        var http = new HttpClient(_handler) { BaseAddress = new Uri("https://api.openai.com/") };
        var options = Options.Create(new OpenAiOptions
        {
            Model = "gpt-test",
            MaxCompletionTokens = 1234,
            ReasoningEffort = "low",
        });
        return new OpenAiAiProvider(http, options);
    }

    private static string CompletionWith(string? content, string finishReason = "stop", string? refusal = null) =>
        JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new { message = new { role = "assistant", content, refusal }, finish_reason = finishReason },
            },
        });

    private static string OpenAiError(string code, string message) =>
        JsonSerializer.Serialize(new { error = new { message, type = code, code } });

    private const string TwoQuestionsJson = """
        {"questions":[
          {"stem":"Q1","options":["a","b","c","d"],"correctOptionIndex":1,"explanation":"e1"},
          {"stem":"Q2","options":["w","x","y","z"],"correctOptionIndex":3,"explanation":null}
        ]}
        """;

    private static readonly GenerateQuestionsRequest AnyQuestionsRequest = new("LINQ", Difficulty.Hard, 2);

    private static readonly CodeFeedbackRequest AnyFeedbackRequest = new(
        ChallengeTitle: "Sum two numbers",
        Prompt: "Read two integers and print their sum.",
        SourceCode: "Console.WriteLine(1);",
        TestCases: [new CodeFeedbackTestCase("1\n2", "3")]);

    private JsonElement RequestJson() => JsonDocument.Parse(_handler.LastRequestBody!).RootElement;

    [Fact]
    public async Task GenerateQuestions_ParsesQuestionsObject_IntoDrafts_AtRequestedDifficulty()
    {
        _handler.RespondWith(CompletionWith(TwoQuestionsJson));

        var drafts = await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        drafts.Should().HaveCount(2);
        drafts[0].Stem.Should().Be("Q1");
        drafts[0].CorrectOptionIndex.Should().Be(1);
        drafts[1].Options.Should().Equal("w", "x", "y", "z");
        drafts[1].Explanation.Should().BeNull();
        drafts.Should().OnlyContain(d => d.Difficulty == Difficulty.Hard);
    }

    [Fact]
    public async Task GenerateQuestions_SendsBearerKey_AndPostsToChatCompletions()
    {
        _handler.RespondWith(CompletionWith(TwoQuestionsJson));

        await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-secret");

        var request = _handler.LastRequest!;
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().Be("/v1/chat/completions");
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("sk-proj-secret");
    }

    [Fact]
    public async Task GenerateQuestions_SendsModelLimitsAndStoreFalse_NeverLegacyParameters()
    {
        _handler.RespondWith(CompletionWith(TwoQuestionsJson));

        await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        var body = RequestJson();
        body.GetProperty("model").GetString().Should().Be("gpt-test");
        body.GetProperty("store").GetBoolean().Should().BeFalse();
        body.GetProperty("max_completion_tokens").GetInt32().Should().Be(1234);
        body.GetProperty("reasoning_effort").GetString().Should().Be("low");
        body.TryGetProperty("max_tokens", out _).Should().BeFalse();
        body.TryGetProperty("temperature", out _).Should().BeFalse();
    }

    [Fact]
    public async Task GenerateQuestions_RequestsStrictJsonSchema_WithFourOptionsPerQuestion()
    {
        _handler.RespondWith(CompletionWith(TwoQuestionsJson));

        await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        var format = RequestJson().GetProperty("response_format");
        format.GetProperty("type").GetString().Should().Be("json_schema");
        var jsonSchema = format.GetProperty("json_schema");
        jsonSchema.GetProperty("strict").GetBoolean().Should().BeTrue();

        var schema = jsonSchema.GetProperty("schema");
        schema.GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        var question = schema.GetProperty("properties").GetProperty("questions").GetProperty("items");
        question.GetProperty("additionalProperties").GetBoolean().Should().BeFalse();
        question.GetProperty("required").EnumerateArray().Select(e => e.GetString())
            .Should().BeEquivalentTo("stem", "options", "correctOptionIndex", "explanation");
        var options = question.GetProperty("properties").GetProperty("options");
        options.GetProperty("minItems").GetInt32().Should().Be(4);
        options.GetProperty("maxItems").GetInt32().Should().Be(4);
    }

    [Fact]
    public async Task GenerateQuestions_PromptIsTheSharedQuestionsPrompt()
    {
        _handler.RespondWith(CompletionWith(TwoQuestionsJson));

        await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        var message = RequestJson().GetProperty("messages")[0];
        message.GetProperty("role").GetString().Should().Be("user");
        message.GetProperty("content").GetString().Should().Be(AiPrompts.BuildQuestionsPrompt(AnyQuestionsRequest));
    }

    [Fact]
    public async Task GenerateQuestions_ToleratesMarkdownFencedJson()
    {
        _handler.RespondWith(CompletionWith("```json\n" + TwoQuestionsJson + "\n```"));

        var drafts = await CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        drafts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateQuestions_MalformedJson_ThrowsAiResponse()
    {
        _handler.RespondWith(CompletionWith("not json at all"));

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task GenerateQuestions_Refusal_ThrowsAiResponse()
    {
        _handler.RespondWith(CompletionWith(null, refusal: "I can't help with that."));

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task GenerateQuestions_TruncatedByLength_ThrowsAiResponse()
    {
        _handler.RespondWith(CompletionWith("""{"questions":[{"stem":"Q1","opt""", finishReason: "length"));

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task GenerateQuestions_NoContent_ThrowsAiResponse()
    {
        _handler.RespondWith("""{"choices":[]}""");

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GenerateQuestions_RejectedKey_ThrowsProviderRejected_InvalidKey(HttpStatusCode status)
    {
        _handler.RespondWith(OpenAiError("invalid_api_key", "Incorrect API key provided."), status);

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "bad-key");

        var thrown = await act.Should().ThrowAsync<AiProviderRejectedException>();
        thrown.Which.Kind.Should().Be(AiProviderKind.OpenAi);
        thrown.Which.Reason.Should().Be(AiRejectionReason.InvalidKey);
    }

    [Theory]
    [InlineData("rate_limit_exceeded")]
    [InlineData("insufficient_quota")]
    public async Task GenerateQuestions_TooManyRequests_ThrowsProviderRejected_QuotaOrRateLimit(string code)
    {
        _handler.RespondWith(OpenAiError(code, "slow down"), HttpStatusCode.TooManyRequests);

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        (await act.Should().ThrowAsync<AiProviderRejectedException>())
            .Which.Reason.Should().Be(AiRejectionReason.QuotaOrRateLimit);
    }

    [Fact]
    public async Task GenerateQuestions_OtherFailure_ThrowsHttpRequest()
    {
        _handler.RespondWith(OpenAiError("server_error", "boom"), HttpStatusCode.InternalServerError);

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, "sk-proj-1");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Rejection_MessageNeverContainsTheKeyOrUpstreamBody()
    {
        const string key = "sk-proj-leaky-0123456789abcdef";
        _handler.RespondWith(OpenAiError("invalid_api_key", $"Incorrect API key provided: {key}"), HttpStatusCode.Unauthorized);

        var act = () => CreateSut().GenerateQuestionsAsync(AnyQuestionsRequest, key);

        var thrown = await act.Should().ThrowAsync<AiProviderRejectedException>();
        thrown.Which.Message.Should().NotContain(key).And.NotContain("89abcdef").And.NotContain("Incorrect API key");
    }

    [Fact]
    public async Task GenerateCodeFeedback_ReturnsModelText_Verbatim_WithoutJsonSchema()
    {
        _handler.RespondWith(CompletionWith("Looks good, but handle empty input."));

        var feedback = await CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "sk-proj-1");

        feedback.Should().Be("Looks good, but handle empty input.");
        var body = RequestJson();
        body.TryGetProperty("response_format", out _).Should().BeFalse();
        body.GetProperty("store").GetBoolean().Should().BeFalse();
        body.GetProperty("messages")[0].GetProperty("content").GetString()
            .Should().Be(AiPrompts.BuildFeedbackPrompt(AnyFeedbackRequest));
    }

    [Fact]
    public async Task GenerateCodeFeedback_RejectedKey_ThrowsProviderRejected_InvalidKey()
    {
        _handler.RespondWith(OpenAiError("invalid_api_key", "Incorrect API key provided."), HttpStatusCode.Unauthorized);

        var act = () => CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "bad-key");

        (await act.Should().ThrowAsync<AiProviderRejectedException>())
            .Which.Reason.Should().Be(AiRejectionReason.InvalidKey);
    }
}
