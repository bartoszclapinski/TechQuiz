using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 30);

    [Fact]
    public void CurrentStreak_NoActiveDays_IsZero()
    {
        StreakCalculator.CurrentStreak([], Today).Should().Be(0);
    }

    [Fact]
    public void CurrentStreak_ActiveToday_CountsConsecutiveBack()
    {
        DateOnly[] days = [Today, Today.AddDays(-1), Today.AddDays(-2)];

        StreakCalculator.CurrentStreak(days, Today).Should().Be(3);
    }

    [Fact]
    public void CurrentStreak_NoActivityToday_StartsFromYesterday_GraceDay()
    {
        // A run that ended yesterday is not yet broken — today still has time to extend it.
        DateOnly[] days = [Today.AddDays(-1), Today.AddDays(-2)];

        StreakCalculator.CurrentStreak(days, Today).Should().Be(2);
    }

    [Fact]
    public void CurrentStreak_GapBeforeYesterday_ResetsRun()
    {
        // No activity today and none yesterday → the run is broken regardless of older days.
        DateOnly[] days = [Today.AddDays(-2), Today.AddDays(-3)];

        StreakCalculator.CurrentStreak(days, Today).Should().Be(0);
    }

    [Fact]
    public void CurrentStreak_IgnoresDuplicatesAndOrder()
    {
        DateOnly[] days = [Today.AddDays(-1), Today, Today, Today.AddDays(-1)];

        StreakCalculator.CurrentStreak(days, Today).Should().Be(2);
    }

    [Fact]
    public void LongestStreak_NoActiveDays_IsZero()
    {
        StreakCalculator.LongestStreak([]).Should().Be(0);
    }

    [Fact]
    public void LongestStreak_ReturnsLongestConsecutiveRun()
    {
        // Runs: {-10}, {-7,-6,-5,-4}, {-1, 0} → longest is 4.
        DateOnly[] days =
        [
            Today.AddDays(-10),
            Today.AddDays(-7), Today.AddDays(-6), Today.AddDays(-5), Today.AddDays(-4),
            Today.AddDays(-1), Today,
        ];

        StreakCalculator.LongestStreak(days).Should().Be(4);
    }

    [Fact]
    public void LongestStreak_SingleDay_IsOne()
    {
        StreakCalculator.LongestStreak([Today]).Should().Be(1);
    }

    [Fact]
    public void LongestStreak_IgnoresDuplicates()
    {
        DateOnly[] days = [Today, Today, Today.AddDays(-1), Today.AddDays(-1)];

        StreakCalculator.LongestStreak(days).Should().Be(2);
    }
}
