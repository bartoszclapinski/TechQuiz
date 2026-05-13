using TechQuiz.Domain;

namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// In-quiz view of a question. Omits <c>Explanation</c> (revealed only on the Result screen)
/// and uses <see cref="OptionDto"/> which has no <c>IsCorrect</c> field.
/// </summary>
public sealed record QuestionDto(
    Guid Id,
    QuestionType Type,
    Difficulty Difficulty,
    string Text,
    IReadOnlyList<OptionDto> Options);
