namespace TechQuiz.Domain;

/// <summary>
/// Pure gamification math (ADR-025): XP per attempt, the level curve, and the Skill IQ metric.
/// Deterministic and dependency-free so every rate is unit-testable in isolation, and nothing is
/// persisted — XP, level and Skill IQ are always a function of the completed attempts we already
/// store. The Application layer orchestrates these over a user's attempts.
/// </summary>
public static class Gamification
{
    /// <summary>XP awarded for each correct answer.</summary>
    public const int XpPerCorrectAnswer = 10;

    /// <summary>The Skill IQ metric is capped at this ceiling.</summary>
    public const int MaxSkillIq = 250;

    /// <summary>XP earned by a single completed attempt: <c>correctCount × 10</c>.</summary>
    public static int XpForAttempt(int correctCount)
    {
        return Math.Max(0, correctCount) * XpPerCorrectAnswer;
    }

    /// <summary>XP required to advance from <paramref name="level"/> to the next: <c>100 + (L−1)×50</c>.</summary>
    public static int XpToAdvance(int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        return 100 + (level - 1) * 50;
    }

    /// <summary>
    /// Resolves total XP into the current level and the progress within it — the exact shape the
    /// "Level 7 · 640 / 800 XP" bar needs. Level is 1-based.
    /// </summary>
    public static LevelProgress LevelFor(int totalXp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalXp);

        var level = 1;
        var remaining = totalXp;
        while (remaining >= XpToAdvance(level))
        {
            remaining -= XpToAdvance(level);
            level++;
        }

        return new LevelProgress(level, remaining, XpToAdvance(level));
    }

    /// <summary>
    /// A single 0–<see cref="MaxSkillIq"/> skill metric: accuracy dominates, quiz volume adds a
    /// capped bump. Returns 0 when the user has completed no quizzes.
    /// </summary>
    public static int SkillIq(double averageScorePercent, int quizCount)
    {
        if (quizCount <= 0)
        {
            return 0;
        }

        var accuracy = Math.Clamp(averageScorePercent, 0d, 100d);
        var volumeBonus = Math.Min(quizCount * 3, 75);
        var raw = accuracy * 1.6 + volumeBonus;
        return (int)Math.Round(Math.Clamp(raw, 0d, MaxSkillIq), MidpointRounding.AwayFromZero);
    }

    /// <summary>A coarse tier label for a Skill IQ value — replaces the mockup's fictional percentile.</summary>
    public static string SkillTier(int skillIq)
    {
        return skillIq switch
        {
            < 100 => "Rising",
            < 150 => "Intermediate",
            < 200 => "Advanced",
            _ => "Expert",
        };
    }
}

/// <summary>Current level plus progress within it. <see cref="XpForNextLevel"/> is the XP the current level spans.</summary>
public readonly record struct LevelProgress(int Level, int XpIntoLevel, int XpForNextLevel);
