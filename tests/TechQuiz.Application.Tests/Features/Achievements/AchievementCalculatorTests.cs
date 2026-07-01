using FluentAssertions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Achievements;

namespace TechQuiz.Application.Tests.Features.Achievements;

public class AchievementCalculatorTests
{
    private static readonly DateTimeOffset Day0 = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    private static CompletedAttemptRow Attempt(
        double score = 50, int answers = 5, int dayOffset = 0) =>
        new(Guid.NewGuid(), "C#", score, Day0.AddDays(dayOffset), answers);

    private static ReviewSessionSummary Review(
        int questions = 5, int correct = 3, int dayOffset = 0) =>
        new(Guid.NewGuid(), Day0.AddDays(dayOffset), questions, correct);

    private static AchievementDto Badge(AchievementsDto result, string key) =>
        result.Items.Single(i => i.Key == key);

    [Fact]
    public void Calculate_WithNoActivity_LocksEveryBadge()
    {
        var result = AchievementCalculator.Calculate([], []);

        result.TotalCount.Should().Be(result.Items.Count);
        result.UnlockedCount.Should().Be(0);
        result.Items.Should().OnlyContain(i => !i.Unlocked && i.Progress == 0);
    }

    [Fact]
    public void Calculate_FirstQuiz_UnlocksOnTheFirstCompletedAttempt()
    {
        Badge(AchievementCalculator.Calculate([], []), "first_quiz").Unlocked.Should().BeFalse();
        Badge(AchievementCalculator.Calculate([Attempt()], []), "first_quiz").Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_TenQuizzes_UnlocksAtExactlyTen_TracksProgressBelow()
    {
        var nine = AchievementCalculator.Calculate(
            Enumerable.Range(0, 9).Select(_ => Attempt()).ToList(), []);
        var nineBadge = Badge(nine, "ten_quizzes");
        nineBadge.Unlocked.Should().BeFalse();
        nineBadge.Progress.Should().Be(9);
        nineBadge.Target.Should().Be(10);

        var ten = AchievementCalculator.Calculate(
            Enumerable.Range(0, 10).Select(_ => Attempt()).ToList(), []);
        Badge(ten, "ten_quizzes").Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_HundredQuestions_CombinesQuizAnswersAndReviewQuestions()
    {
        var attempts = new[] { Attempt(answers: 60) };
        var reviews = new[] { Review(questions: 40, correct: 10) };

        var badge = Badge(AchievementCalculator.Calculate(attempts, reviews), "hundred_questions");

        badge.Progress.Should().Be(100);
        badge.Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_ClampsProgressToTarget_WhenRawExceedsIt()
    {
        var attempts = new[] { Attempt(answers: 250) };

        var badge = Badge(AchievementCalculator.Calculate(attempts, []), "hundred_questions");

        badge.Progress.Should().Be(100);
    }

    [Fact]
    public void Calculate_PerfectQuiz_UnlocksOnlyAtOneHundredPercent()
    {
        Badge(AchievementCalculator.Calculate([Attempt(score: 99)], []), "perfect_quiz")
            .Unlocked.Should().BeFalse();
        Badge(AchievementCalculator.Calculate([Attempt(score: 100)], []), "perfect_quiz")
            .Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_FirstReview_UnlocksOnTheFirstSession()
    {
        Badge(AchievementCalculator.Calculate([], [Review()]), "first_review")
            .Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_FiftyReviewCorrect_SumsCorrectAcrossSessions()
    {
        var below = new[] { Review(questions: 30, correct: 25), Review(questions: 30, correct: 24) };
        Badge(AchievementCalculator.Calculate([], below), "fifty_review_correct")
            .Unlocked.Should().BeFalse();

        var atTarget = new[] { Review(questions: 30, correct: 25), Review(questions: 30, correct: 25) };
        Badge(AchievementCalculator.Calculate([], atTarget), "fifty_review_correct")
            .Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_SevenDayStreak_UnionsQuizAndReviewActiveDays()
    {
        // Quiz on days 0..3, review on days 4..6 — neither alone is a 7-day run, but their union is.
        var attempts = Enumerable.Range(0, 4).Select(d => Attempt(dayOffset: d)).ToList();
        var reviews = Enumerable.Range(4, 3).Select(d => Review(dayOffset: d)).ToList();

        var badge = Badge(AchievementCalculator.Calculate(attempts, reviews), "seven_day_streak");

        badge.Progress.Should().Be(7);
        badge.Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Calculate_SevenDayStreak_StaysLockedBelowSevenConsecutiveDays()
    {
        var attempts = Enumerable.Range(0, 6).Select(d => Attempt(dayOffset: d)).ToList();

        var badge = Badge(AchievementCalculator.Calculate(attempts, []), "seven_day_streak");

        badge.Progress.Should().Be(6);
        badge.Unlocked.Should().BeFalse();
    }

    [Fact]
    public void Calculate_RollsUpUnlockedCount()
    {
        // One completed quiz unlocks exactly first_quiz and nothing else.
        var result = AchievementCalculator.Calculate([Attempt(score: 50, answers: 5)], []);

        result.UnlockedCount.Should().Be(1);
        result.Items.Count(i => i.Unlocked).Should().Be(1);
    }
}
