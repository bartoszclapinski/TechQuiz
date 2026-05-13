namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Lightweight summary of one quiz attempt — for history list / dashboard tiles.
/// Score data is not included here (computed on-demand when the user opens the
/// attempt detail view).
/// </summary>
public sealed record AttemptHistoryItemDto(
    Guid AttemptId,
    Guid QuizId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    bool IsCompleted);
