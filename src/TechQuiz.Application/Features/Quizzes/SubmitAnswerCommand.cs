using MediatR;

namespace TechQuiz.Application.Features.Quizzes;

/// <summary>
/// Records an answer for a question within an active quiz attempt. <c>SelectedOptionId</c>
/// may be <c>null</c> for "unanswered" (counted as wrong by scoring).
/// </summary>
public sealed record SubmitAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    Guid? SelectedOptionId) : IRequest;
