using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
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

    [Fact]
    public async Task GenerateQuestions_NonSuccessStatus_ThrowsHttpRequest()
    {
        _handler.RespondWith("""{"error":"unauthorized"}""", HttpStatusCode.Unauthorized);

        var act = () => CreateSut().GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "bad-key");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private string _body = "{}";
        private HttpStatusCode _status = HttpStatusCode.OK;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public void RespondWith(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            _body = body;
            _status = status;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
        }
    }
}
