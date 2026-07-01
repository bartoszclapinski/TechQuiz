namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A past review session's full graded detail, returned by <c>GetReviewSessionDetailQuery</c> and
/// re-rendered on the session-detail screen. Scoped to the owner (the handler authorizes before
/// returning). Carries no owner id — see <see cref="ReviewSessionDetailResult"/> for the repository
/// projection that does, used only for the ownership check.
/// </summary>
public sealed record ReviewSessionDetailDto(
    Guid Id,
    DateTimeOffset CompletedAt,
    IReadOnlyList<ReviewSessionItemDto> Items);
