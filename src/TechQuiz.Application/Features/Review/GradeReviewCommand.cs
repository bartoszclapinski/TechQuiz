using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

/// <summary>
/// One answer the user gave during a daily-review session. <c>SelectedOptionId</c> may be
/// <c>null</c> for "skipped" (graded as incorrect).
/// </summary>
public sealed record ReviewAnswerInput(Guid QuestionId, Guid? SelectedOptionId);

/// <summary>
/// Grades a daily-review session. Stateless — no <c>QuizAttempt</c> is created or persisted; the
/// result is a pure function of the submitted answers and the questions' correct options.
/// </summary>
public sealed record GradeReviewCommand(IReadOnlyList<ReviewAnswerInput> Answers)
    : IRequest<IReadOnlyList<ReviewGradeResultDto>>;
