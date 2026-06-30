using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Dashboard;

namespace TechQuiz.Application.Tests.Features.Dashboard;

public class GetDashboardSummaryQueryHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly Guid _userId = Guid.NewGuid();

    // Fixed "now" so streak/sparkline math is deterministic. Today (UTC) is 2026-06-15.
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private GetDashboardSummaryQueryHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        _timeProvider.GetUtcNow().Returns(Now);
        return new GetDashboardSummaryQueryHandler(_quizRepository, _userContext, _timeProvider);
    }

    private void GivenAttempts(params CompletedAttemptRow[] rows) =>
        _quizRepository
            .GetCompletedAttemptsWithCategoryAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(rows);

    private static CompletedAttemptRow Row(
        string category, double score, DateTimeOffset completedAt, int answers = 0) =>
        new(Guid.NewGuid(), category, score, completedAt, answers);

    private static DateTimeOffset Day(int year, int month, int day) =>
        new(year, month, day, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_NoCompletedAttempts_ReturnsEmptyState()
    {
        GivenAttempts();

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(0);
        result.ActivitySparkline.Should().HaveCount(14).And.OnlyContain(c => c == 0);
        result.ScoreOverTime.Should().BeEmpty();
        result.CategoryStrength.Should().BeEmpty();
        result.TotalQuestionsAnswered.Should().Be(0);
        result.AverageScore.Should().BeNull();
        result.RecentActivity.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AveragesScore_AndSumsAnswerCounts()
    {
        GivenAttempts(
            Row("C#", 40d, Day(2026, 6, 15), answers: 3),
            Row("C#", 80d, Day(2026, 6, 15), answers: 2));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.AverageScore.Should().Be(60d);
        result.TotalQuestionsAnswered.Should().Be(5);
    }

    [Fact]
    public async Task Handle_CategoryStrength_AveragesPerCategory_OrderedByScoreDescending()
    {
        GivenAttempts(
            Row("SQL", 50d, Day(2026, 6, 15)),
            Row("SQL", 70d, Day(2026, 6, 15)),
            Row("C#", 90d, Day(2026, 6, 15)),
            Row("EF Core", 30d, Day(2026, 6, 15)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CategoryStrength.Select(c => c.Category).Should().Equal("C#", "SQL", "EF Core");
        result.CategoryStrength.Select(c => c.AverageScore).Should().Equal(90d, 60d, 30d);
        result.CategoryStrength.Single(c => c.Category == "SQL").AttemptCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ScoreOverTime_MapsRowsPreservingRepositoryOrder()
    {
        var t1 = Day(2026, 6, 13);
        var t2 = Day(2026, 6, 14);
        GivenAttempts(Row("C#", 40d, t1), Row("C#", 80d, t2));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.ScoreOverTime.Select(p => p.CompletedAt).Should().Equal(t1, t2);
        result.ScoreOverTime.Select(p => p.ScorePercentage).Should().Equal(40d, 80d);
    }

    [Fact]
    public async Task Handle_RecentActivity_ReturnsMostRecentFirst_CappedAtFive()
    {
        GivenAttempts(
            Row("C#", 10d, Day(2026, 6, 1)),
            Row("C#", 20d, Day(2026, 6, 2)),
            Row("C#", 30d, Day(2026, 6, 3)),
            Row("C#", 40d, Day(2026, 6, 4)),
            Row("C#", 50d, Day(2026, 6, 5)),
            Row("C#", 60d, Day(2026, 6, 6)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.RecentActivity.Should().HaveCount(5);
        result.RecentActivity.Select(r => r.ScorePercentage).Should().Equal(60d, 50d, 40d, 30d, 20d);
    }

    [Fact]
    public async Task Handle_Streak_CountsConsecutiveDaysEndingToday()
    {
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),
            Row("C#", 80d, Day(2026, 6, 14)),
            Row("C#", 80d, Day(2026, 6, 13)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(3);
    }

    [Fact]
    public async Task Handle_Streak_HasOneDayGrace_WhenTodayNotYetPlayed()
    {
        // No attempt today (06-15); most recent is yesterday. The grace keeps the run alive.
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 14)),
            Row("C#", 80d, Day(2026, 6, 13)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Streak_BreaksOnAGap()
    {
        // Active today and two days ago, but 06-14 is missing — streak is just today.
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),
            Row("C#", 80d, Day(2026, 6, 13)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Streak_IsZero_WhenNoRecentActivity()
    {
        GivenAttempts(Row("C#", 80d, Day(2026, 6, 10)));

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Sparkline_CountsPerDayInWindow_TodayLast_IgnoringOlder()
    {
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),   // today → index 13
            Row("C#", 80d, Day(2026, 6, 14)),   // index 12
            Row("C#", 80d, Day(2026, 6, 14)),   // index 12 again
            Row("C#", 80d, Day(2026, 6, 1)));   // before the 14-day window → ignored

        var result = await CreateSut().Handle(new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.ActivitySparkline.Should().HaveCount(14);
        result.ActivitySparkline[13].Should().Be(1);
        result.ActivitySparkline[12].Should().Be(2);
        result.ActivitySparkline.Sum().Should().Be(3);
    }

    [Fact]
    public async Task Handle_Week_IncludesAttemptsWithinSevenDays_ExcludesOlder()
    {
        // Today is 06-15. Week cutoff = 06-09 (today − 6). 06-09 is in; 06-08 is out.
        // Rows are oldest→newest, mirroring the repository's ordering contract.
        GivenAttempts(
            Row("C#", 10d, Day(2026, 6, 8), answers: 5),
            Row("C#", 70d, Day(2026, 6, 9), answers: 3),
            Row("C#", 90d, Day(2026, 6, 15), answers: 2));

        var result = await CreateSut().Handle(
            new GetDashboardSummaryQuery(DashboardRange.Week), CancellationToken.None);

        result.ScoreOverTime.Select(p => p.ScorePercentage).Should().Equal(70d, 90d);
        result.AverageScore.Should().Be(80d);
        result.TotalQuestionsAnswered.Should().Be(5);
        result.CategoryStrength.Single().AttemptCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Month_IncludesAttemptsWithinThirtyDays_ExcludesOlder()
    {
        // Today is 06-15. Month cutoff = 05-17 (today − 29). 05-17 is in; 05-16 is out.
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),
            Row("C#", 60d, Day(2026, 5, 17)),
            Row("C#", 20d, Day(2026, 5, 16)));

        var result = await CreateSut().Handle(
            new GetDashboardSummaryQuery(DashboardRange.Month), CancellationToken.None);

        result.ScoreOverTime.Should().HaveCount(2);
        result.AverageScore.Should().Be(70d);
    }

    [Fact]
    public async Task Handle_All_AppliesNoCutoff()
    {
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),
            Row("C#", 40d, Day(2026, 1, 1)));

        var result = await CreateSut().Handle(
            new GetDashboardSummaryQuery(DashboardRange.All), CancellationToken.None);

        result.ScoreOverTime.Should().HaveCount(2);
        result.AverageScore.Should().Be(60d);
    }

    [Fact]
    public async Task Handle_StreakAndSparkline_StayAllTime_RegardlessOfRange()
    {
        // A 3-day run ending today, plus an old attempt outside the Week window. Filtering to Week
        // drops the old attempt from the aggregations but must not shorten the streak or empty the
        // sparkline (both are all-time "state as of now").
        GivenAttempts(
            Row("C#", 80d, Day(2026, 6, 15)),
            Row("C#", 80d, Day(2026, 6, 14)),
            Row("C#", 80d, Day(2026, 6, 13)),
            Row("C#", 80d, Day(2026, 5, 1)));

        var result = await CreateSut().Handle(
            new GetDashboardSummaryQuery(DashboardRange.Week), CancellationToken.None);

        result.CurrentStreakDays.Should().Be(3);
        result.ActivitySparkline.Sum().Should().Be(3);
    }

    [Fact]
    public async Task Handle_RangeEmptyButAllTimeHasData_EmptyAggregations_ButSparklineStillCounts()
    {
        // Only an old attempt: the Week range is empty, so averageScore is null and lists are empty —
        // but the sparkline still reflects all-time activity (the attempt is within 14 days).
        GivenAttempts(Row("C#", 80d, Day(2026, 6, 5), answers: 4));

        var result = await CreateSut().Handle(
            new GetDashboardSummaryQuery(DashboardRange.Week), CancellationToken.None);

        result.AverageScore.Should().BeNull();
        result.ScoreOverTime.Should().BeEmpty();
        result.CategoryStrength.Should().BeEmpty();
        result.RecentActivity.Should().BeEmpty();
        result.TotalQuestionsAnswered.Should().Be(0);
        result.ActivitySparkline.Sum().Should().Be(1);
    }
}
