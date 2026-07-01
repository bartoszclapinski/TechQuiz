using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Application.Tests.Features.Review;

public class GetReviewStatsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    public GetReviewStatsQueryHandlerTests()
    {
        _userContext.UserId.Returns(UserId);
        _timeProvider.GetUtcNow().Returns(Now);
    }

    private GetReviewStatsQueryHandler CreateSut() => new(_quizRepository, _userContext, _timeProvider);

    private void GivenSessions(params ReviewSessionSummary[] summaries) =>
        _quizRepository.GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(summaries);

    [Fact]
    public async Task Handle_NoSessions_ReturnsZeros_AndNullAccuracy()
    {
        GivenSessions();

        var stats = await CreateSut().Handle(new GetReviewStatsQuery(), CancellationToken.None);

        stats.Should().BeEquivalentTo(new ReviewStatsDto(0, 0, null, 0, 0, false));
    }

    [Fact]
    public async Task Handle_AggregatesTotalsAndAccuracy()
    {
        GivenSessions(
            new ReviewSessionSummary(Now.AddDays(-2), QuestionCount: 10, CorrectCount: 7),
            new ReviewSessionSummary(Now.AddDays(-1), QuestionCount: 10, CorrectCount: 8));

        var stats = await CreateSut().Handle(new GetReviewStatsQuery(), CancellationToken.None);

        stats.TotalSessions.Should().Be(2);
        stats.TotalQuestionsReviewed.Should().Be(20);
        stats.AccuracyPercentage.Should().Be(75d); // (7 + 8) / 20
    }

    [Fact]
    public async Task Handle_ComputesCurrentAndBestStreak_FromSessionDays()
    {
        // A 3-day run ending today, and an older isolated day.
        GivenSessions(
            new ReviewSessionSummary(Now.AddDays(-10), 5, 5),
            new ReviewSessionSummary(Now.AddDays(-2), 5, 5),
            new ReviewSessionSummary(Now.AddDays(-1), 5, 5),
            new ReviewSessionSummary(Now, 5, 5));

        var stats = await CreateSut().Handle(new GetReviewStatsQuery(), CancellationToken.None);

        stats.CurrentStreakDays.Should().Be(3);
        stats.BestStreakDays.Should().Be(3);
        stats.ReviewedToday.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoSessionToday_ReviewedTodayFalse_StreakUsesGraceDay()
    {
        // Last review was yesterday: not reviewed today, but the streak isn't broken yet (grace).
        GivenSessions(
            new ReviewSessionSummary(Now.AddDays(-2), 5, 5),
            new ReviewSessionSummary(Now.AddDays(-1), 5, 5));

        var stats = await CreateSut().Handle(new GetReviewStatsQuery(), CancellationToken.None);

        stats.ReviewedToday.Should().BeFalse();
        stats.CurrentStreakDays.Should().Be(2);
    }
}
