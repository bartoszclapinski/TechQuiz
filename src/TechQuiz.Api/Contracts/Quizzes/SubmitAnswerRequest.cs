namespace TechQuiz.Api.Contracts.Quizzes;

/// <summary>
/// Body for recording an answer. <c>SelectedOptionId</c> may be <c>null</c> to record an
/// explicit "unanswered" (scored as wrong). The attempt id comes from the route.
/// </summary>
public sealed record SubmitAnswerRequest(Guid QuestionId, Guid? SelectedOptionId);
