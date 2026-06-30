import { useQuery } from '@tanstack/react-query'
import { fetchDashboardSummary } from './api'
import { dashboardKey } from './query-keys'

// The whole Dashboard is one read, so it's a single TanStack *query* — cached and self-refetching,
// unlike the code-challenge Run/Submit/Feedback *mutations* (which are actions that spend a call and
// cache nothing). One query fans its payload out to every tile; we never fetch per-tile.
export function useDashboard() {
  return useQuery({ queryKey: dashboardKey(), queryFn: fetchDashboardSummary })
}
