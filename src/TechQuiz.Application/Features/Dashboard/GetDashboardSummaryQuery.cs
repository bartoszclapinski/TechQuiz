using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Dashboard;

/// <summary>
/// Aggregate read for the Dashboard screen, scoped to the current user. Takes no parameters yet —
/// the Week/Month/All time-range filter (iteration 2.3) will add one to this same query.
/// </summary>
public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
