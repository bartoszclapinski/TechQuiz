namespace TechQuiz.Domain;

/// <summary>
/// Pure selection logic for the "Daily review" queue. Given the user's answer history (one
/// <see cref="ReviewCandidate"/> per question occurrence), it keeps each question's <em>latest</em>
/// answer, drops the ones answered correctly (those are "learned"), and ranks the rest so that harder
/// and longer-forgotten questions surface first — the minimal spaced-repetition signal.
/// </summary>
public static class ReviewSelector
{
    private const double RecencyCapDays = 30d;

    public static IReadOnlyList<Guid> SelectDailyReview(
        IEnumerable<ReviewCandidate> candidates,
        int count,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (count <= 0)
        {
            return [];
        }

        return candidates
            .GroupBy(c => c.QuestionId)
            .Select(group => group.MaxBy(c => c.LastAnsweredAt)!)
            .Where(c => !c.WasCorrect)
            .OrderByDescending(c => Weight(c, now))
            .ThenByDescending(c => DaysSince(c.LastAnsweredAt, now))
            .ThenBy(c => c.QuestionId)
            .Take(count)
            .Select(c => c.QuestionId)
            .ToList();
    }

    private static double Weight(ReviewCandidate candidate, DateTimeOffset now) =>
        DifficultyFactor(candidate.Difficulty) + Math.Min(DaysSince(candidate.LastAnsweredAt, now), RecencyCapDays);

    private static int DifficultyFactor(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 1,
        Difficulty.Medium => 2,
        Difficulty.Hard => 3,
        _ => 1,
    };

    private static double DaysSince(DateTimeOffset when, DateTimeOffset now) =>
        Math.Max(0d, (now - when).TotalDays);
}
