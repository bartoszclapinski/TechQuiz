namespace TechQuiz.Domain;

/// <summary>
/// Pure day-streak math shared by the quiz Dashboard and the daily-review stats. Days are caller-
/// supplied <see cref="DateOnly"/> values (UTC by convention); duplicates and ordering don't matter.
/// </summary>
public static class StreakCalculator
{
    /// <summary>
    /// Consecutive active days counting back from <paramref name="today"/>. A one-day grace keeps an
    /// unplayed <em>today</em> from breaking a run: with no activity today we start from yesterday, so
    /// the streak only resets once a full day has passed with no activity.
    /// </summary>
    public static int CurrentStreak(IEnumerable<DateOnly> activeDays, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(activeDays);
        var set = activeDays as ISet<DateOnly> ?? activeDays.ToHashSet();

        var day = set.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (set.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }

        return streak;
    }

    /// <summary>
    /// The longest run of consecutive active days ever recorded (the personal best).
    /// </summary>
    public static int LongestStreak(IEnumerable<DateOnly> activeDays)
    {
        ArgumentNullException.ThrowIfNull(activeDays);
        var ordered = activeDays.Distinct().OrderBy(d => d).ToList();
        if (ordered.Count == 0)
        {
            return 0;
        }

        var best = 1;
        var run = 1;
        for (var i = 1; i < ordered.Count; i++)
        {
            run = ordered[i] == ordered[i - 1].AddDays(1) ? run + 1 : 1;
            if (run > best)
            {
                best = run;
            }
        }

        return best;
    }
}
