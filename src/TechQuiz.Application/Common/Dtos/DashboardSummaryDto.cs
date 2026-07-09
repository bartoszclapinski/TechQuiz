namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Everything the Dashboard bento grid needs, in one payload. Computed from the user's completed
/// attempts. Best / Needs-practice tiles aren't separate fields — the UI derives them as the
/// max/min of <see cref="CategoryStrength"/>. Empty state: no completed attempts ⇒
/// <see cref="AverageScore"/> null, empty lists, <see cref="CurrentStreakDays"/> 0.
/// </summary>
public sealed record DashboardSummaryDto(
    int CurrentStreakDays,
    IReadOnlyList<int> ActivitySparkline,
    IReadOnlyList<ScorePointDto> ScoreOverTime,
    IReadOnlyList<CategoryStrengthDto> CategoryStrength,
    int TotalQuestionsAnswered,
    double? AverageScore,
    IReadOnlyList<RecentActivityItemDto> RecentActivity,
    GamificationDto Gamification);

/// <summary>One point on the score-over-time line — a completed attempt's score at its completion.</summary>
public sealed record ScorePointDto(DateTimeOffset CompletedAt, double ScorePercentage);

/// <summary>One radar axis — a category's mean score across the user's completed attempts in it.</summary>
public sealed record CategoryStrengthDto(string Category, double AverageScore, int AttemptCount);

/// <summary>One recent-activity row — a recently completed attempt with its category and score.</summary>
public sealed record RecentActivityItemDto(
    Guid AttemptId,
    string Category,
    double ScorePercentage,
    DateTimeOffset CompletedAt);
