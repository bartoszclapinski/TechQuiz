using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

/// <summary>
/// Reads one past review session's graded detail for the session-detail screen. User-scoped: the
/// handler throws when the session belongs to another user (403) or does not exist (404).
/// </summary>
public sealed record GetReviewSessionDetailQuery(Guid SessionId)
    : IRequest<ReviewSessionDetailDto>;
