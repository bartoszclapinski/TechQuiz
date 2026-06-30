namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A single completed quiz attempt flattened with its category name — the read row the
/// dashboard aggregate is computed from. Not an API contract: it feeds
/// <c>GetDashboardSummaryQueryHandler</c>, which aggregates these in memory into the
/// tiles. Only completed, scored attempts are ever projected into this shape.
/// </summary>
public sealed record CompletedAttemptRow(
    Guid AttemptId,
    string Category,
    double ScorePercentage,
    DateTimeOffset CompletedAt,
    int AnswerCount);
