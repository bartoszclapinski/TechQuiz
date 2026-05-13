using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Quizzes;

/// <summary>
/// Result of <c>StartQuizCommand</c>. Contains the new attempt id and the ordered
/// questions for the client to play through. Questions are projected via
/// <see cref="QuestionDto"/> which does not expose <c>IsCorrect</c>.
/// </summary>
public sealed record QuizSessionDto(
    Guid AttemptId,
    IReadOnlyList<QuestionDto> Questions);
