using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Ai;

public class GenerateQuestionsCommandHandlerTests
{
    private readonly IAiProviderResolver _resolver = Substitute.For<IAiProviderResolver>();
    private readonly IAiProvider _provider = Substitute.For<IAiProvider>();
    private readonly IAiKeyStore _keyStore = Substitute.For<IAiKeyStore>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Guid _userId = Guid.NewGuid();

    private GenerateQuestionsCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new GenerateQuestionsCommandHandler(_resolver, _keyStore, _userContext);
    }

    private void GivenKey(AiProviderKind kind, string key) =>
        _keyStore.GetAsync(_userId, kind, Arg.Any<CancellationToken>()).Returns(key);

    private static GeneratedQuestionDraft AnyDraft() =>
        new("What is a CLR?", ["A runtime", "A linter", "A db", "A shell"], 0, Difficulty.Easy, null);

    [Fact]
    public async Task Handle_ResolvesRequestedProvider_AndReturnsItsDrafts()
    {
        var drafts = new[] { AnyDraft() };
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        _resolver.Resolve(AiProviderKind.Anthropic).Returns(_provider);
        _provider
            .GenerateQuestionsAsync(
                Arg.Any<GenerateQuestionsRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(drafts);

        var result = await CreateSut().Handle(
            new GenerateQuestionsCommand("CLR", Difficulty.Medium, 3, AiProviderKind.Anthropic),
            CancellationToken.None);

        result.Provider.Should().Be(AiProviderKind.Anthropic);
        result.Questions.Should().BeEquivalentTo(drafts);
        _resolver.Received(1).Resolve(AiProviderKind.Anthropic);
    }

    [Fact]
    public async Task Handle_PassesCurrentUsersKeyForProviderToTheProvider()
    {
        GivenKey(AiProviderKind.Anthropic, "sk-ant-secret");
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _provider
            .GenerateQuestionsAsync(
                Arg.Any<GenerateQuestionsRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([AnyDraft()]);

        await CreateSut().Handle(
            new GenerateQuestionsCommand("EF Core", Difficulty.Hard, 5, AiProviderKind.Anthropic),
            CancellationToken.None);

        await _provider.Received(1).GenerateQuestionsAsync(
            Arg.Is<GenerateQuestionsRequest>(r =>
                r.Topic == "EF Core" && r.Difficulty == Difficulty.Hard && r.Count == 5),
            "sk-ant-secret",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoKeyConfigured_ThrowsMissingAiKey_AndNeverCallsProvider()
    {
        _keyStore.GetAsync(_userId, AiProviderKind.Anthropic, Arg.Any<CancellationToken>())
            .Returns((string?)null);
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);

        var act = () => CreateSut().Handle(
            new GenerateQuestionsCommand("SQL", Difficulty.Easy, 1, AiProviderKind.Anthropic),
            CancellationToken.None);

        (await act.Should().ThrowAsync<MissingAiKeyException>())
            .Which.Kind.Should().Be(AiProviderKind.Anthropic);
        await _provider.DidNotReceive().GenerateQuestionsAsync(
            Arg.Any<GenerateQuestionsRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnregisteredProvider_PropagatesResolverError()
    {
        _resolver.Resolve(AiProviderKind.OpenRouter)
            .Throws(new UnknownAiProviderException(AiProviderKind.OpenRouter));

        var act = () => CreateSut().Handle(
            new GenerateQuestionsCommand("SQL", Difficulty.Easy, 1, AiProviderKind.OpenRouter),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnknownAiProviderException>();
    }

    [Fact]
    public async Task Handle_ProviderThrows_PropagatesUnchanged()
    {
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _provider
            .GenerateQuestionsAsync(
                Arg.Any<GenerateQuestionsRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("rate limited"));

        var act = () => CreateSut().Handle(
            new GenerateQuestionsCommand("ASP.NET", Difficulty.Medium, 2, AiProviderKind.Anthropic),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("rate limited");
    }
}
