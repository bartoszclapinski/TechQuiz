namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// In-quiz view of an answer option. Intentionally omits <c>IsCorrect</c> (per CLAUDE.md
/// Hard Rule #4 — must never leak the correct option while a quiz is active).
/// </summary>
public sealed record OptionDto(
    Guid Id,
    string Text,
    int OrderIndex);
