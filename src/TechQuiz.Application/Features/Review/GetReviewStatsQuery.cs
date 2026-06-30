using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

/// <summary>
/// Aggregate stats for the current user's daily-review activity. User-scoped (no parameters);
/// the handler reads the identity from <c>IUserContext</c>.
/// </summary>
public sealed record GetReviewStatsQuery : IRequest<ReviewStatsDto>;
