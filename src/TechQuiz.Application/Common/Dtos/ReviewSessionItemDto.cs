using TechQuiz.Domain;

namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// One graded question inside a past review session's detail view. Correctness is derived on read
/// (matching <see cref="SelectedOptionId"/> against the question's correct option) — never stored.
/// Unlike the active-review <see cref="ReviewQuestionDto"/> this legitimately carries correctness: the
/// session is already submitted, so hard rule #4 no longer applies (mirrors the grade reveal).
/// </summary>
public sealed record ReviewSessionItemDto(
    Guid QuestionId,
    string QuestionText,
    string Category,
    Difficulty Difficulty,
    IReadOnlyList<OptionDto> Options,
    Guid? SelectedOptionId,
    Guid CorrectOptionId,
    bool IsCorrect,
    string Explanation);
