using FluentAssertions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Dashboard;

namespace TechQuiz.Application.Tests.Features.Dashboard;

public sealed class GamificationCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 7, 9);

    private static CompletedAttemptRow Attempt(double score, int answers, DateOnly day) =>
        new(Guid.NewGuid(), "C#/.NET", score, new DateTimeOffset(day, TimeOnly.MinValue, TimeSpan.Zero), answers);

    [Fact]
    public void Calculate_returns_empty_gamification_when_no_attempts()
    {
        var result = GamificationCalculator.Calculate([], Today);

        result.Should().Be(new GamificationDto(
            TotalXp: 0, Level: 1, XpIntoLevel: 0, XpForNextLevel: 100,
            SkillIq: 0, SkillIqWeeklyDelta: 0, Tier: "Rising"));
    }

    [Fact]
    public void Calculate_sums_xp_from_recovered_correct_counts()
    {
        // Two perfect 10-question quizzes → 10 correct each → 200 XP → level 2 (100 spent, 100 into 150).
        var attempts = new[]
        {
            Attempt(100, 10, Today.AddDays(-1)),
            Attempt(100, 10, Today),
        };

        var result = GamificationCalculator.Calculate(attempts, Today);

        result.TotalXp.Should().Be(200);
        result.Level.Should().Be(2);
        result.XpIntoLevel.Should().Be(100);
        result.XpForNextLevel.Should().Be(150);
    }

    [Fact]
    public void Calculate_derives_skill_iq_from_average_score_and_volume()
    {
        // avg 80, 5 quizzes → 80×1.6 + min(15,75) = 128 + 15 = 143 → Intermediate.
        var attempts = Enumerable.Range(0, 5)
            .Select(i => Attempt(80, 8, Today.AddDays(-i)))
            .ToList();

        var result = GamificationCalculator.Calculate(attempts, Today);

        result.SkillIq.Should().Be(143);
        result.Tier.Should().Be("Intermediate");
    }

    [Fact]
    public void Calculate_weekly_delta_is_metric_now_minus_metric_before_this_week()
    {
        // Older attempts (outside the last 7 days) establish a prior Skill IQ; this week's attempts move it.
        var attempts = new[]
        {
            Attempt(60, 10, Today.AddDays(-20)),
            Attempt(60, 10, Today.AddDays(-15)),
            Attempt(95, 10, Today.AddDays(-2)),
            Attempt(95, 10, Today),
        };

        var result = GamificationCalculator.Calculate(attempts, Today);

        // Recompute the "before this week" metric (attempts older than today-6) and confirm the delta.
        var priorAvg = new[] { 60d, 60d }.Average();
        var priorSkillIq = TechQuiz.Domain.Gamification.SkillIq(priorAvg, 2);
        result.SkillIqWeeklyDelta.Should().Be(result.SkillIq - priorSkillIq);
        result.SkillIqWeeklyDelta.Should().BePositive();
    }
}
