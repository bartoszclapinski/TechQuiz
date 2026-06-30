namespace TechQuiz.Application.Features.Dashboard;

/// <summary>
/// Time window the Dashboard aggregations are scoped to. <see cref="All"/> applies no cutoff.
/// Streak and the activity sparkline ignore this — they are always all-time "state as of now".
/// </summary>
public enum DashboardRange
{
    Week,
    Month,
    All,
}
