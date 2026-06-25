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
    private readonly IPooledQuestionRepository _pooledQuestions = Substitute.For<IPooledQuestionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly Guid _userId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 6, 25, 12, 0, 0, TimeSpan.Zero);

    private IReadOnlyList<PooledQuestion> _captured = [];

    private GenerateQuestionsCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        _timeProvider.GetUtcNow().Returns(Now);
        _pooledQuestions.AddRangeAsync(
            Arg.Do<IEnumerable<PooledQuestion>>(q => _captured = q.ToList()),
            Arg.Any<CancellationToken>());
        return new GenerateQuestionsCommandHandler(
            _resolver, _keyStore, _userContext, _pooledQuestions, _unitOfWork, _timeProvider);
    }

    private void GivenKey(AiProviderKind kind, string key) =>
        _keyStore.GetAsync(_userId, kind, Arg.Any<CancellationToken>()).Returns(key);

    private void GivenProviderReturns(params GeneratedQuestionDraft[] drafts)
    {
        _resolver.Resolve(Arg.Any<AiProviderKind>()).Returns(_provider);
        _provider
            .GenerateQuestionsAsync(
                Arg.Any<GenerateQuestionsRequest>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(drafts);
    }

    private static GeneratedQuestionDraft AnyDraft() =>
        new("What is a CLR?", ["A runtime", "A linter", "A db", "A shell"], 0, Difficulty.Easy, null);

    [Fact]
    public async Task Handle_ResolvesRequestedProvider_AndReturnsAnswerKeyFreeSummaries()
    {
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        GivenProviderReturns(AnyDraft());

        var result = await CreateSut().Handle(
            new GenerateQuestionsCommand("CLR", Difficulty.Medium, 3, AiProviderKind.Anthropic),
            CancellationToken.None);

        result.Provider.Should().Be(AiProviderKind.Anthropic);
        result.Questions.Should().ContainSingle();
        var summary = result.Questions[0];
        summary.Id.Should().NotBeEmpty();
        summary.Stem.Should().Be("What is a CLR?");
        summary.Options.Should().Equal("A runtime", "A linter", "A db", "A shell");
        summary.Difficulty.Should().Be(Difficulty.Easy);
        _resolver.Received(1).Resolve(AiProviderKind.Anthropic);
    }

    [Fact]
    public async Task Handle_PassesCurrentUsersKeyForProviderToTheProvider()
    {
        GivenKey(AiProviderKind.Anthropic, "sk-ant-secret");
        GivenProviderReturns(AnyDraft());

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
    public async Task Handle_PersistsEachDraftAsDraftWithAttributionAndCorrectOption()
    {
        GivenKey(AiProviderKind.Anthropic, "sk-ant-1");
        GivenProviderReturns(
            new GeneratedQuestionDraft(
                "Which is a value type?",
                ["string", "object", "int", "dynamic"],
                CorrectOptionIndex: 2,
                Difficulty.Medium,
                "int is a struct."));

        var result = await CreateSut().Handle(
            new GenerateQuestionsCommand("C# types", Difficulty.Medium, 1, AiProviderKind.Anthropic),
            CancellationToken.None);

        await _pooledQuestions.Received(1).AddRangeAsync(
            Arg.Any<IEnumerable<PooledQuestion>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _captured.Should().ContainSingle();
        var persisted = _captured[0];
        persisted.Status.Should().Be(PooledQuestionStatus.Draft);
        persisted.CreatedByUserId.Should().Be(_userId);
        persisted.Provider.Should().Be("Anthropic");
        persisted.Topic.Should().Be("C# types");
        persisted.GeneratedAtUtc.Should().Be(Now.UtcDateTime);
        persisted.Text.Should().Be("Which is a value type?");
        persisted.Options.Single(o => o.IsCorrect).Text.Should().Be("int");
        persisted.Options.Where(o => !o.IsCorrect).Should().HaveCount(3);

        result.Questions[0].Id.Should().Be(persisted.Id);
    }

    [Fact]
    public async Task Handle_NoKeyConfigured_ThrowsMissingAiKey_AndNeverCallsProviderOrPersists()
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
        await _pooledQuestions.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<PooledQuestion>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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
    public async Task Handle_ProviderThrows_PropagatesUnchanged_AndNeverPersists()
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
        await _pooledQuestions.DidNotReceive().AddRangeAsync(
            Arg.Any<IEnumerable<PooledQuestion>>(), Arg.Any<CancellationToken>());
    }
}
