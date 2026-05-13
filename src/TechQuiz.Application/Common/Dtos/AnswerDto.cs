namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Represents a submitted answer. <c>SelectedOptionId</c> can be <c>null</c> for an
/// unanswered question (counted as wrong by the scoring rules).
/// </summary>
public sealed record AnswerDto(
    Guid QuestionId,
    Guid? SelectedOptionId,
    DateTimeOffset SubmittedAt);
