namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Per-question outcome of grading a daily-review session. Returned only after the user submits
/// their answers — this is where correctness and the explanation are first revealed (hard rule #4
/// allows the reveal once the session is over, mirroring the live-quiz Result screen).
/// </summary>
public sealed record ReviewGradeResultDto(
    Guid QuestionId,
    Guid? SelectedOptionId,
    Guid CorrectOptionId,
    bool IsCorrect,
    string Explanation);
