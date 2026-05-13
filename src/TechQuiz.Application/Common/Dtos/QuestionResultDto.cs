using TechQuiz.Domain;

namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Post-complete view of a single question with the user's answer and correctness flag.
/// Includes the <c>Explanation</c> (hidden during the quiz) so the result screen can show it.
/// </summary>
public sealed record QuestionResultDto(
    Guid QuestionId,
    string Text,
    Difficulty Difficulty,
    string Explanation,
    IReadOnlyList<OptionResultDto> Options,
    Guid? UserSelectedOptionId,
    bool IsCorrect);
