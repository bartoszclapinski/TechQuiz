import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { fetchDashboardSummary, type DashboardRange } from './api'
import { dashboardKey } from './query-keys'

// The whole Dashboard is one read, so it's a single TanStack *query* — cached and self-refetching,
// unlike the code-challenge Run/Submit/Feedback *mutations* (which are actions that spend a call and
// cache nothing). One query fans its payload out to every tile; we never fetch per-tile.
//
// `keepPreviousData` holds the last range's payload on screen while a newly-selected range loads, so
// switching the Week/Month/All tabs swaps numbers in place instead of flashing the loading state.
export function useDashboard(range: DashboardRange) {
  return useQuery({
    queryKey: dashboardKey(range),
    queryFn: () => fetchDashboardSummary(range),
    placeholderData: keepPreviousData,
  })
}
