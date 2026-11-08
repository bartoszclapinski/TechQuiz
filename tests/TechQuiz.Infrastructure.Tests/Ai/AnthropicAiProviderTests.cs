using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Ai;

namespace TechQuiz.Infrastructure.Tests.Ai;

public sealed class AnthropicAiProviderTests
{
    private readonly CapturingHandler _handler = new();

    private AnthropicAiProvider CreateSut()
    {
        var http = new HttpClient(_handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        var options = Options.Create(new AnthropicOptions { Model = "claude-test" });
        return new AnthropicAiProvider(http, options);
    }

    private static string MessageWith(string text)
    {
        var escaped = System.Text.Json.JsonSerializer.Serialize(text);
        return $$"""{"content":[{"type":"text","text":{{escaped}}}]}""";
    }

    private const string TwoQuestionsJson = """
        [
          {"stem":"Q1","options":["a","b","c","d"],"correctOptionIndex":1,"explanation":"e1"},
          {"stem":"Q2","options":["w","x","y","z"],"correctOptionIndex":3,"explanation":"e2"}
        ]
        """;

    private const string TwoQuestionsObjectJson = """
        {"questions":[
          {"stem":"Q1","options":["a","b","c","d"],"correctOptionIndex":1,"explanation":"e1"},
          {"stem":"Q2","options":["w","x","y","z"],"correctOptionIndex":3,"explanation":null}
        ]}
        """;

    [Fact]
    public async Task GenerateQuestions_ParsesQuestionsObject_IntoDrafts_AtRequestedDifficulty()
    {
        _handler.RespondWith(MessageWith(TwoQuestionsObjectJson));

        var drafts = await CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Medium, 2), "sk-ant-1");

        drafts.Should().HaveCount(2);
        drafts[0].Stem.Should().Be("Q1");
        drafts[0].CorrectOptionIndex.Should().Be(1);
        drafts[1].Options.Should().Equal("w", "x", "y", "z");
        drafts[1].Explanation.Should().BeNull();
        drafts.Should().OnlyContain(d => d.Difficulty == Difficulty.Medium);
    }

    [Fact]
    public async Task GenerateQuestions_PromptAsksForQuestionsObject()
    {
        _handler.RespondWith(MessageWith(TwoQuestionsObjectJson));

        await CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 2), "sk-ant-1");

        using var body = System.Text.Json.JsonDocument.Parse(_handler.LastRequestBody!);
        var prompt = body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString();
        prompt.Should().Contain("\"questions\"");
    }

    [Fact]
    public async Task GenerateQuestions_EmptyQuestionsObject_ThrowsAiResponse()
    {
        _handler.RespondWith(MessageWith("""{"questions":[]}"""));

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    // Tolerance: a model may still answer with the older bare-array shape.
    [Fact]
    public async Task GenerateQuestions_ParsesJsonArray_IntoDrafts_AtRequestedDifficulty()
    {
        _handler.RespondWith(MessageWith(TwoQuestionsJson));

        var drafts = await CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Hard, 2), "sk-ant-1");

        drafts.Should().HaveCount(2);
        drafts[0].Stem.Should().Be("Q1");
        drafts[0].CorrectOptionIndex.Should().Be(1);
        drafts[0].Options.Should().Equal("a", "b", "c", "d");
        drafts[0].Explanation.Should().Be("e1");
        drafts.Should().OnlyContain(d => d.Difficulty == Difficulty.Hard);
    }

    [Fact]
    public async Task GenerateQuestions_SendsKeyAndVersionHeaders_AndPostsToMessagesEndpoint()
    {
        _handler.RespondWith(MessageWith(TwoQuestionsJson));

        await CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-secret");

        var request = _handler.LastRequest!;
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().Be("/v1/messages");
        request.Headers.GetValues("x-api-key").Should().ContainSingle().Which.Should().Be("sk-ant-secret");
        request.Headers.GetValues("anthropic-version").Should().ContainSingle();
        _handler.LastRequestBody.Should().Contain("claude-test");
    }

    [Fact]
    public async Task GenerateQuestions_ToleratesMarkdownFencedJson()
    {
        _handler.RespondWith(MessageWith("```json\n" + TwoQuestionsJson + "\n```"));

        var drafts = await CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Medium, 2), "sk-ant-1");

        drafts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateQuestions_NoTextBlock_ThrowsAiResponse()
    {
        _handler.RespondWith("""{"content":[]}""");

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task GenerateQuestions_MalformedJson_ThrowsAiResponse()
    {
        _handler.RespondWith(MessageWith("not json at all"));

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GenerateQuestions_RejectedKey_ThrowsProviderRejected_InvalidKey(HttpStatusCode status)
    {
        _handler.RespondWith(AnthropicError("authentication_error", "invalid x-api-key"), status);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "bad-key");

        var thrown = await act.Should().ThrowAsync<AiProviderRejectedException>();
        thrown.Which.Kind.Should().Be(AiProviderKind.Anthropic);
        thrown.Which.Reason.Should().Be(AiRejectionReason.InvalidKey);
    }

    [Fact]
    public async Task GenerateQuestions_TooManyRequests_ThrowsProviderRejected_QuotaOrRateLimit()
    {
        _handler.RespondWith(AnthropicError("rate_limit_error", "slow down"), HttpStatusCode.TooManyRequests);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        (await act.Should().ThrowAsync<AiProviderRejectedException>())
            .Which.Reason.Should().Be(AiRejectionReason.QuotaOrRateLimit);
    }

    [Fact]
    public async Task GenerateQuestions_LowCreditBalance_ThrowsProviderRejected_QuotaOrRateLimit()
    {
        _handler.RespondWith(
            AnthropicError("invalid_request_error", "Your credit balance is too low to access the Anthropic API."),
            HttpStatusCode.BadRequest);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        (await act.Should().ThrowAsync<AiProviderRejectedException>())
            .Which.Reason.Should().Be(AiRejectionReason.QuotaOrRateLimit);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GenerateQuestions_OtherFailure_ThrowsHttpRequest(HttpStatusCode status)
    {
        _handler.RespondWith(AnthropicError("api_error", "something broke"), status);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "sk-ant-1");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Rejection_MessageNeverContainsTheKeyOrUpstreamBody()
    {
        const string key = "sk-ant-leaky-0123456789abcdef";
        _handler.RespondWith(AnthropicError("authentication_error", $"invalid key {key}"), HttpStatusCode.Unauthorized);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), key);

        var thrown = await act.Should().ThrowAsync<AiProviderRejectedException>();
        thrown.Which.Message.Should().NotContain(key).And.NotContain("89abcdef").And.NotContain("invalid key");
    }

    private static string AnthropicError(string type, string message) =>
        $$$"""{"type":"error","error":{"type":"{{{type}}}","message":{{{System.Text.Json.JsonSerializer.Serialize(message)}}}}}""";

    private static readonly CodeFeedbackRequest AnyFeedbackRequest = new(
        ChallengeTitle: "Sum two numbers",
        Prompt: "Read two integers from stdin and print their sum.",
        SourceCode: "var n = int.Parse(Console.ReadLine());",
        TestCases: [new CodeFeedbackTestCase("2\n3", "5"), new CodeFeedbackTestCase("0\n0", "0")]);

    [Fact]
    public async Task GenerateCodeFeedback_ReturnsModelTextBlock_Verbatim()
    {
        _handler.RespondWith(MessageWith("You never handle empty input — guard against it."));

        var feedback = await CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "sk-ant-1");

        feedback.Should().Be("You never handle empty input — guard against it.");
    }

    [Fact]
    public async Task GenerateCodeFeedback_SendsKeyHeader_AndPostsSubmissionToMessagesEndpoint()
    {
        _handler.RespondWith(MessageWith("ok"));

        await CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "sk-ant-secret");

        var request = _handler.LastRequest!;
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().Be("/v1/messages");
        request.Headers.GetValues("x-api-key").Should().ContainSingle().Which.Should().Be("sk-ant-secret");
        _handler.LastRequestBody.Should().Contain("int.Parse(Console.ReadLine())");
    }

    [Fact]
    public async Task GenerateCodeFeedback_PromptInstructsModelNotToQuoteExpectedOutputs()
    {
        _handler.RespondWith(MessageWith("ok"));

        await CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "sk-ant-1");

        // Anti-leak guard (spirit of hard rule #4): the prompt must tell the model not to
        // restate the exact expected outputs back to the user.
        _handler.LastRequestBody.Should().Contain("NOT quote or restate");
    }

    [Fact]
    public async Task GenerateCodeFeedback_NoTextBlock_ThrowsAiResponse()
    {
        _handler.RespondWith("""{"content":[]}""");

        var act = () => CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "sk-ant-1");

        await act.Should().ThrowAsync<AiResponseException>();
    }

    [Fact]
    public async Task GenerateCodeFeedback_RejectedKey_ThrowsProviderRejected_InvalidKey()
    {
        _handler.RespondWith(AnthropicError("authentication_error", "invalid x-api-key"), HttpStatusCode.Unauthorized);

        var act = () => CreateSut().GenerateCodeFeedbackAsync(AnyFeedbackRequest, "bad-key");

        (await act.Should().ThrowAsync<AiProviderRejectedException>())
            .Which.Reason.Should().Be(AiRejectionReason.InvalidKey);
    }
}
