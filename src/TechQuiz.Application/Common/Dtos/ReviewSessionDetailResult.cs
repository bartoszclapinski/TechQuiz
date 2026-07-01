namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// Repository projection of a review session's detail. Identical to <see cref="ReviewSessionDetailDto"/>
/// but carries the <see cref="UserId"/> so the query handler can authorize ownership (403 vs 404)
/// before mapping to the owner-safe response DTO.
/// </summary>
public sealed record ReviewSessionDetailResult(
    Guid Id,
    Guid UserId,
    DateTimeOffset CompletedAt,
    IReadOnlyList<ReviewSessionItemDto> Items);
