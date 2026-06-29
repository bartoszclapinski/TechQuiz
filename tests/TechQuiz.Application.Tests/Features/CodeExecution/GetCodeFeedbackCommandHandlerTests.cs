using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Application.Features.CodeExecution;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.CodeExecution;

public class GetCodeFeedbackCommandHandlerTests
{
    private readonly ICodeChallengeCatalog _catalog = Substitute.For<ICodeChallengeCatalog>();
    private readonly IAiProviderResolver _resolver = Substitute.For<IAiProviderResolver>();
    private readonly IAiProvider _provider = Substitute.For<IAiProvider>();
    private readonly IAiKeyStore _keyStore = Substitute.For<IAiKeyStore>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Guid _userId = Guid.NewGuid();

    private static readonly Guid ChallengeId = Guid.NewGuid();

    private GetCodeFeedbackCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new GetCodeFeedbackCommandHandler(_catalog, _resolver, _keyStore, _userContext);
    }

    private static CodeChallenge AnyChallenge() => CodeChallenge.Create(
        ChallengeId,
        Guid.NewGuid(),
        Difficulty.Easy,
        "Sum two numbers",
        "Read two integers and print their sum.",
        starterCode: null,
        testCases:
        [
            new CodeChallengeTestCase("2\n3", "5", 0),
            new CodeChallengeTestCase("0\n0", "0", 1),
        ]);

    private void GivenChallenge(CodeChallenge challenge) =>
        _catalog.Find(ChallengeId).Returns(challenge);

    private void GivenKey(AiProviderKind kind, string key) =>
        _keyStore.GetAsync(_userId, kind, Arg.Any<CancellationToken>()).Returns(key);

    private void GivenProviderReturns(string feedback)
    {
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _provider
            .GenerateCodeFeedbackAsync(
                Arg.Any<CodeFeedbackRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(feedback);
    }

    private GetCodeFeedbackCommand Command(string source = "code") =>
        new(ChallengeId, source, AiProviderKind.Anthropic);

    [Fact]
    public async Task Handle_ResolvesProvider_LoadsKey_AndReturnsFeedbackWithProvider()
    {
        GivenChallenge(AnyChallenge());
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        GivenProviderReturns("Guard against empty input.");

        var result = await CreateSut().Handle(Command(), CancellationToken.None);

        result.Feedback.Should().Be("Guard against empty input.");
        result.Provider.Should().Be(AiProviderKind.Anthropic);
        _resolver.Received(1).Resolve(AiProviderKind.Anthropic);
    }

    [Fact]
    public async Task Handle_PassesChallengeAndSubmissionAndHiddenCasesToProvider_WithCallersKey()
    {
        GivenChallenge(AnyChallenge());
        GivenKey(AiProviderKind.Anthropic, "sk-ant-secret");
        GivenProviderReturns("ok");

        await CreateSut().Handle(Command("var x = 1;"), CancellationToken.None);

        await _provider.Received(1).GenerateCodeFeedbackAsync(
            Arg.Is<CodeFeedbackRequest>(r =>
                r.ChallengeTitle == "Sum two numbers"
                && r.Prompt == "Read two integers and print their sum."
                && r.SourceCode == "var x = 1;"
                && r.TestCases.Count == 2
                && r.TestCases[0].Stdin == "2\n3"
                && r.TestCases[0].ExpectedStdout == "5"),
            "sk-ant-secret",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownChallenge_ThrowsKeyNotFound_AndNeverCallsProvider()
    {
        _catalog.Find(ChallengeId).Returns((CodeChallenge?)null);

        var act = () => CreateSut().Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _provider.DidNotReceive().GenerateCodeFeedbackAsync(
            Arg.Any<CodeFeedbackRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoKeyConfigured_ThrowsMissingAiKey_AndNeverCallsProvider()
    {
        GivenChallenge(AnyChallenge());
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _keyStore.GetAsync(_userId, AiProviderKind.Anthropic, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var act = () => CreateSut().Handle(Command(), CancellationToken.None);

        (await act.Should().ThrowAsync<MissingAiKeyException>())
            .Which.Kind.Should().Be(AiProviderKind.Anthropic);
        await _provider.DidNotReceive().GenerateCodeFeedbackAsync(
            Arg.Any<CodeFeedbackRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProviderThrows_PropagatesUnchanged()
    {
        GivenChallenge(AnyChallenge());
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _provider
            .GenerateCodeFeedbackAsync(
                Arg.Any<CodeFeedbackRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("rate limited"));

        var act = () => CreateSut().Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("rate limited");
    }
}
