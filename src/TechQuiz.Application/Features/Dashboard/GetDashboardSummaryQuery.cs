using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Dashboard;

/// <summary>
/// Aggregate read for the Dashboard screen, scoped to the current user and a time <paramref name="Range"/>.
/// The range scopes the aggregations (score over time, category strength, recent activity, totals);
/// streak and the sparkline stay all-time regardless.
/// </summary>
public sealed record GetDashboardSummaryQuery(DashboardRange Range) : IRequest<DashboardSummaryDto>;
