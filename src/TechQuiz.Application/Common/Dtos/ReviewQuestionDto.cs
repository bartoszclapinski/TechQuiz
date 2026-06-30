using TechQuiz.Domain;

namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A question served in the Daily review queue. Mirrors the active-quiz <see cref="QuestionDto"/>
/// (so 2.6 can run review as a quiz-like flow) and adds <see cref="Category"/> for the mixed-category
/// display. Reuses <see cref="OptionDto"/>, which has no <c>IsCorrect</c> — correctness must never leak
/// while the question is being re-attempted (Hard Rule #4).
/// </summary>
public sealed record ReviewQuestionDto(
    Guid Id,
    QuestionType Type,
    Difficulty Difficulty,
    string Text,
    string Category,
    IReadOnlyList<OptionDto> Options);
