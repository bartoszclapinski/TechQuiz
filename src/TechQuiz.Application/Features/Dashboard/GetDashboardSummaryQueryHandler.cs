using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Dashboard;

public sealed class GetDashboardSummaryQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    TimeProvider timeProvider)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private const int SparklineDays = 14;
    private const int RecentActivityCount = 5;

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await quizRepository.GetCompletedAttemptsWithCategoryAsync(
            userContext.UserId, cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        // Streak and the sparkline are all-time "state as of now" — the range filter never touches them.
        var streak = ComputeStreak(rows, today);
        var sparkline = ComputeSparkline(rows, today);

        // Every other tile is scoped to the selected range.
        var scoped = FilterByRange(rows, request.Range, today);

        if (scoped.Count == 0)
        {
            return new DashboardSummaryDto(
                CurrentStreakDays: streak,
                ActivitySparkline: sparkline,
                ScoreOverTime: [],
                CategoryStrength: [],
                TotalQuestionsAnswered: 0,
                AverageScore: null,
                RecentActivity: []);
        }

        var scoreOverTime = scoped
            .Select(r => new ScorePointDto(r.CompletedAt, r.ScorePercentage))
            .ToList();

        var categoryStrength = scoped
            .GroupBy(r => r.Category)
            .Select(g => new CategoryStrengthDto(g.Key, g.Average(r => r.ScorePercentage), g.Count()))
            .OrderByDescending(c => c.AverageScore)
            .ToList();

        var recentActivity = scoped
            .OrderByDescending(r => r.CompletedAt)
            .Take(RecentActivityCount)
            .Select(r => new RecentActivityItemDto(
                r.AttemptId, r.Category, r.ScorePercentage, r.CompletedAt))
            .ToList();

        return new DashboardSummaryDto(
            CurrentStreakDays: streak,
            ActivitySparkline: sparkline,
            ScoreOverTime: scoreOverTime,
            CategoryStrength: categoryStrength,
            TotalQuestionsAnswered: scoped.Sum(r => r.AnswerCount),
            AverageScore: scoped.Average(r => r.ScorePercentage),
            RecentActivity: recentActivity);
    }

    // Date-based, UTC cutoff (consistent with streak/sparkline): an attempt is in range when its
    // completed date is on or after the cutoff. Week is a 7-day window (today − 6), Month a 30-day
    // window (today − 29); All applies no cutoff.
    private static IReadOnlyList<CompletedAttemptRow> FilterByRange(
        IReadOnlyList<CompletedAttemptRow> rows, DashboardRange range, DateOnly today)
    {
        if (range == DashboardRange.All)
        {
            return rows;
        }

        var cutoff = range == DashboardRange.Week ? today.AddDays(-6) : today.AddDays(-29);
        return rows
            .Where(r => DateOnly.FromDateTime(r.CompletedAt.UtcDateTime) >= cutoff)
            .ToList();
    }

    // Consecutive days with at least one completed attempt, counting back from today (with a one-day
    // grace) — the shared StreakCalculator also backs the daily-review streak, keeping the two in parity.
    private static int ComputeStreak(IReadOnlyList<CompletedAttemptRow> rows, DateOnly today)
    {
        var activeDays = rows.Select(r => DateOnly.FromDateTime(r.CompletedAt.UtcDateTime));
        return StreakCalculator.CurrentStreak(activeDays, today);
    }

    // Per-day completed-attempt counts for the last SparklineDays days, oldest→newest, index
    // SparklineDays-1 being today. Days with no activity are zero, so the array is always full length.
    private static int[] ComputeSparkline(IReadOnlyList<CompletedAttemptRow> rows, DateOnly today)
    {
        var counts = new int[SparklineDays];
        var start = today.AddDays(-(SparklineDays - 1));

        foreach (var row in rows)
        {
            var day = DateOnly.FromDateTime(row.CompletedAt.UtcDateTime);
            if (day < start || day > today)
            {
                continue;
            }

            counts[day.DayNumber - start.DayNumber]++;
        }

        return counts;
    }
}
