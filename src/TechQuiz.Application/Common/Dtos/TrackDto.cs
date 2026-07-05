namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A top-level track with its ordered subcategories. Shape returned by the categories endpoint —
/// the web catalogue renders tracks and drills into their categories (ADR-023).
/// </summary>
public sealed record TrackDto(
    Guid Id,
    string Name,
    string Description,
    string IconCode,
    int Position,
    IReadOnlyList<CategoryDto> Categories);
