using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Dashboard;

/// <summary>
/// Turns a user's completed attempts into the dashboard gamification block (ADR-025) — total XP,
/// level progress, and Skill IQ with its this-week delta. Pure and deterministic (the Domain
/// <see cref="Gamification"/> math plus a rounding of each attempt's correct-answer count from its
/// stored score) so nothing is persisted and the numbers can never drift from the attempts.
/// </summary>
public static class GamificationCalculator
{
    public static GamificationDto Calculate(IReadOnlyList<CompletedAttemptRow> attempts, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(attempts);

        var totalXp = attempts.Sum(XpFor);
        var level = Gamification.LevelFor(totalXp);
        var skillIq = SkillIqOf(attempts);

        // "▲ N this week" — how much Skill IQ moved on the back of the last 7 days of activity:
        // the metric now minus the metric computed over everything before this week.
        var weekStart = today.AddDays(-6);
        var beforeThisWeek = attempts
            .Where(a => DateOnly.FromDateTime(a.CompletedAt.UtcDateTime) < weekStart)
            .ToList();
        var weeklyDelta = skillIq - SkillIqOf(beforeThisWeek);

        return new GamificationDto(
            TotalXp: totalXp,
            Level: level.Level,
            XpIntoLevel: level.XpIntoLevel,
            XpForNextLevel: level.XpForNextLevel,
            SkillIq: skillIq,
            SkillIqWeeklyDelta: weeklyDelta,
            Tier: Gamification.SkillTier(skillIq));
    }

    private static int XpFor(CompletedAttemptRow attempt)
    {
        // The read row stores the score % and answer count, not the raw correct count — recover it.
        var correct = (int)Math.Round(
            attempt.ScorePercentage / 100d * attempt.AnswerCount, MidpointRounding.AwayFromZero);
        return Gamification.XpForAttempt(correct);
    }

    private static int SkillIqOf(IReadOnlyList<CompletedAttemptRow> attempts)
    {
        if (attempts.Count == 0)
        {
            return 0;
        }

        return Gamification.SkillIq(attempts.Average(a => a.ScorePercentage), attempts.Count);
    }
}
