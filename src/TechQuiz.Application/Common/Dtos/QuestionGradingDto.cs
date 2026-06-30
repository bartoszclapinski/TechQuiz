namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// The grading-side projection of a question: just enough to score a submitted answer and explain
/// it — the correct option's id and the explanation. Unlike <see cref="ReviewQuestionDto"/> this
/// deliberately carries correctness, so it never leaves the grade path.
/// </summary>
public sealed record QuestionGradingDto(
    Guid Id,
    Guid CorrectOptionId,
    string Explanation);
