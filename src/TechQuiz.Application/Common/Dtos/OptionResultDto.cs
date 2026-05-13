namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Post-complete view of an option — exposes <c>IsCorrect</c> for the result screen.
/// Distinct from <see cref="OptionDto"/> (in-quiz view) so we cannot accidentally
/// leak the correct answer while a quiz is still active.
/// </summary>
public sealed record OptionResultDto(
    Guid Id,
    string Text,
    int OrderIndex,
    bool IsCorrect);
