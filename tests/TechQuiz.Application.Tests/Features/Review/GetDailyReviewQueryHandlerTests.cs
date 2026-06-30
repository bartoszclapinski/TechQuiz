using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Review;

public class GetDailyReviewQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public GetDailyReviewQueryHandlerTests()
    {
        _timeProvider.GetUtcNow().Returns(Now);
    }

    private GetDailyReviewQueryHandler CreateSut() => new(_quizRepository, _userContext, _timeProvider);

    private static ReviewCandidate Wrong(Guid id, Difficulty difficulty) =>
        new(id, difficulty, Now.AddDays(-2), WasCorrect: false);

    private static ReviewQuestionDto Question(Guid id) =>
        new(id, QuestionType.MultipleChoice, Difficulty.Easy, "Q?", "C#", []);

    [Fact]
    public async Task Handle_ReordersQuestionsToMatchSelectorWeighting()
    {
        var userId = Guid.NewGuid();
        var hard = Guid.NewGuid();
        var easy = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetReviewCandidatesAsync(userId, Arg.Any<CancellationToken>())
            .Returns([Wrong(easy, Difficulty.Easy), Wrong(hard, Difficulty.Hard)]);
        // Repository returns content in the "wrong" order; handler must restore selector order.
        _quizRepository.GetReviewQuestionsByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([Question(easy), Question(hard)]);

        var result = await CreateSut().Handle(new GetDailyReviewQuery(), CancellationToken.None);

        // Hard outweighs Easy at equal recency, so it comes first.
        result.Select(q => q.Id).Should().Equal(hard, easy);
    }

    [Fact]
    public async Task Handle_NoWrongQuestions_ReturnsEmpty_AndSkipsContentFetch()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetReviewCandidatesAsync(userId, Arg.Any<CancellationToken>())
            .Returns([new ReviewCandidate(Guid.NewGuid(), Difficulty.Easy, Now.AddDays(-1), WasCorrect: true)]);

        var result = await CreateSut().Handle(new GetDailyReviewQuery(), CancellationToken.None);

        result.Should().BeEmpty();
        await _quizRepository.DidNotReceive().GetReviewQuestionsByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HonorsCount_FetchingOnlyTheTopN()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        var candidates = new[]
        {
            Wrong(Guid.NewGuid(), Difficulty.Hard),
            Wrong(Guid.NewGuid(), Difficulty.Medium),
            Wrong(Guid.NewGuid(), Difficulty.Easy),
        };
        _quizRepository.GetReviewCandidatesAsync(userId, Arg.Any<CancellationToken>()).Returns(candidates);
        _quizRepository.GetReviewQuestionsByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => ((IReadOnlyCollection<Guid>)callInfo[0]).Select(Question).ToList());

        await CreateSut().Handle(new GetDailyReviewQuery(Count: 2), CancellationToken.None);

        await _quizRepository.Received(1).GetReviewQuestionsByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ScopesCandidateFetchToCurrentUser()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetReviewCandidatesAsync(userId, Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateSut().Handle(new GetDailyReviewQuery(), CancellationToken.None);

        await _quizRepository.Received(1).GetReviewCandidatesAsync(userId, Arg.Any<CancellationToken>());
    }
}
