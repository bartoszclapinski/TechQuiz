using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Achievements;

/// <summary>
/// Turns a user's completed quiz attempts and review-session summaries into the fixed badge catalogue.
/// Pure and deterministic — no repository, clock, or user context — so the thresholds are unit-testable
/// in isolation. Every badge is monotonic (a count or best-streak that only ever rises), so
/// <c>Progress = min(raw, Target)</c> and <c>Unlocked = raw &gt;= Target</c>. Nothing is persisted; a
/// badge's state is a function of the numbers we already store (decision (a)).
/// </summary>
public static class AchievementCalculator
{
    private sealed record Definition(
        string Key,
        string Title,
        string Description,
        string Group,
        int Target,
        Func<Metrics, int> Progress);

    private sealed record Metrics(
        int QuizCount,
        int QuestionsAnswered,
        int PerfectQuizzes,
        int ReviewCount,
        int ReviewCorrect,
        int BestStreak);

    private static readonly IReadOnlyList<Definition> Catalogue =
    [
        new("first_quiz", "First Steps", "Complete your first quiz", "quiz", 1, m => m.QuizCount),
        new("ten_quizzes", "Quiz Regular", "Complete 10 quizzes", "quiz", 10, m => m.QuizCount),
        new("hundred_questions", "Centurion", "Answer 100 questions", "quiz", 100, m => m.QuestionsAnswered),
        new("perfect_quiz", "Flawless", "Score 100% on a quiz", "quiz", 1, m => m.PerfectQuizzes),
        new("first_review", "First Review", "Complete a daily review", "review", 1, m => m.ReviewCount),
        new("fifty_review_correct", "Sharpshooter", "Get 50 review answers right", "review", 50, m => m.ReviewCorrect),
        new("seven_day_streak", "Streak Keeper", "Reach a 7-day streak", "streak", 7, m => m.BestStreak),
    ];

    public static AchievementsDto Calculate(
        IReadOnlyList<CompletedAttemptRow> attempts,
        IReadOnlyList<ReviewSessionSummary> reviews)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(reviews);

        var metrics = Measure(attempts, reviews);

        var items = Catalogue
            .Select(d =>
            {
                var raw = d.Progress(metrics);
                return new AchievementDto(
                    d.Key, d.Title, d.Description, d.Group, d.Target,
                    Progress: Math.Min(raw, d.Target),
                    Unlocked: raw >= d.Target);
            })
            .ToList();

        return new AchievementsDto(
            UnlockedCount: items.Count(i => i.Unlocked),
            TotalCount: items.Count,
            Items: items);
    }

    private static Metrics Measure(
        IReadOnlyList<CompletedAttemptRow> attempts,
        IReadOnlyList<ReviewSessionSummary> reviews)
    {
        // Streak is over the union of every day the user did *anything* (quiz or review); best, not
        // current, so an earned streak badge never un-earns after a lapse (decision (c)).
        var activeDays = attempts
            .Select(a => DateOnly.FromDateTime(a.CompletedAt.UtcDateTime))
            .Concat(reviews.Select(r => DateOnly.FromDateTime(r.CompletedAt.UtcDateTime)));

        return new Metrics(
            QuizCount: attempts.Count,
            QuestionsAnswered: attempts.Sum(a => a.AnswerCount) + reviews.Sum(r => r.QuestionCount),
            PerfectQuizzes: attempts.Count(a => a.ScorePercentage >= 100d),
            ReviewCount: reviews.Count,
            ReviewCorrect: reviews.Sum(r => r.CorrectCount),
            BestStreak: StreakCalculator.LongestStreak(activeDays));
    }
}
