import type { DashboardRange } from './api'

// The dashboard is a single cached read keyed for the current user's session, with the time-range
// (2.3) folded in so each range caches independently — switching tabs reuses a prior range's data.
export const dashboardKey = (range: DashboardRange) => ['dashboard', range] as const
