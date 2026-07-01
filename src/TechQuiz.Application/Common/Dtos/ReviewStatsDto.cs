namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Aggregate stats for the daily-review feature, shown on its own Dashboard tile. Review has fewer
/// questions than a quiz, so it earns its own numbers rather than folding into the quiz aggregates.
/// </summary>
public sealed record ReviewStatsDto(
    int TotalSessions,
    int TotalQuestionsReviewed,
    double? AccuracyPercentage,
    int CurrentStreakDays,
    int BestStreakDays,
    bool ReviewedToday);
