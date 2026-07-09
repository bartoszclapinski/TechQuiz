using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public sealed class GamificationTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 10)]
    [InlineData(13, 130)]
    [InlineData(-5, 0)] // never negative
    public void XpForAttempt_awards_ten_per_correct_answer(int correctCount, int expected)
    {
        Gamification.XpForAttempt(correctCount).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 150)]
    [InlineData(3, 200)]
    [InlineData(7, 400)]
    public void XpToAdvance_grows_by_fifty_per_level(int level, int expected)
    {
        Gamification.XpToAdvance(level).Should().Be(expected);
    }

    [Fact]
    public void XpToAdvance_rejects_levels_below_one()
    {
        var act = () => Gamification.XpToAdvance(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 1, 0, 100)]     // brand new
    [InlineData(50, 1, 50, 100)]   // partway through level 1
    [InlineData(100, 2, 0, 150)]   // exactly leveled up
    [InlineData(250, 3, 0, 200)]   // 100 + 150 consumed
    [InlineData(300, 3, 50, 200)]  // into level 3
    [InlineData(450, 4, 0, 250)]   // 100 + 150 + 200 consumed
    public void LevelFor_resolves_total_xp_into_level_and_progress(
        int totalXp, int expectedLevel, int expectedInto, int expectedForNext)
    {
        var progress = Gamification.LevelFor(totalXp);

        progress.Level.Should().Be(expectedLevel);
        progress.XpIntoLevel.Should().Be(expectedInto);
        progress.XpForNextLevel.Should().Be(expectedForNext);
    }

    [Fact]
    public void LevelFor_rejects_negative_xp()
    {
        var act = () => Gamification.LevelFor(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SkillIq_is_zero_without_any_quizzes()
    {
        Gamification.SkillIq(averageScorePercent: 90, quizCount: 0).Should().Be(0);
    }

    [Fact]
    public void SkillIq_blends_accuracy_and_capped_volume()
    {
        // 84 × 1.6 = 134.4, volume bonus min(20×3, 75) = 60 → 194.4 → 194
        Gamification.SkillIq(averageScorePercent: 84, quizCount: 20).Should().Be(194);
    }

    [Fact]
    public void SkillIq_caps_volume_bonus_and_overall_value()
    {
        // Volume bonus caps at 75 (30 quizzes and 100 quizzes both give 75); overall caps at 250.
        Gamification.SkillIq(100, 30).Should().Be(235); // 160 + 75
        Gamification.SkillIq(100, 100).Should().Be(235); // volume still 75
        Gamification.SkillIq(100, 30).Should().BeLessThanOrEqualTo(Gamification.MaxSkillIq);
    }

    [Fact]
    public void SkillIq_clamps_accuracy_into_range()
    {
        // Out-of-range accuracy is clamped, not extrapolated.
        Gamification.SkillIq(averageScorePercent: 130, quizCount: 1)
            .Should().Be(Gamification.SkillIq(100, 1));
    }

    [Theory]
    [InlineData(80, "Rising")]
    [InlineData(120, "Intermediate")]
    [InlineData(180, "Advanced")]
    [InlineData(220, "Expert")]
    public void SkillTier_labels_the_metric(int skillIq, string expected)
    {
        Gamification.SkillTier(skillIq).Should().Be(expected);
    }
}
