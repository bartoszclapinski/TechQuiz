namespace TechQuiz.Application.Features.History;

/// <summary>
/// Server-side sort key for the History page. Bound case-insensitively by name on the
/// query string (<c>?sortBy=date</c> / <c>?sortBy=score</c>).
/// </summary>
public enum HistorySortField
{
    Date,
    Score,
}
