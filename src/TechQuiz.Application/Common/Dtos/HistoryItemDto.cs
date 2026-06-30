namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// One row of the History page: a completed, scored attempt with its category name,
/// score, and completion time. Links to the existing result screen via <see cref="AttemptId"/>.
/// </summary>
public sealed record HistoryItemDto(
    Guid AttemptId,
    string Category,
    double ScorePercentage,
    DateTimeOffset CompletedAt);
