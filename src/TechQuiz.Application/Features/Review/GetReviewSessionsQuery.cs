using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

/// <summary>
/// Lists the current user's completed review sessions (newest first) for the review hub's history.
/// </summary>
public sealed record GetReviewSessionsQuery : IRequest<IReadOnlyList<ReviewSessionSummary>>;
