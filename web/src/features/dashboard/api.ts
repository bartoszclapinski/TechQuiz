import { apiClient } from '../../lib/api-client'

// Mirrors the API's DashboardSummaryDto (one aggregate payload feeding every bento tile).
// Best / Needs-practice aren't fields — they're the max/min of `categoryStrength`, derived here
// on the client. Empty state: `averageScore` is null (no completed attempts) ⇒ empty lists,
// `currentStreakDays` 0, a 14-long zero sparkline.
export type ScorePoint = {
  completedAt: string
  scorePercentage: number
}

export type CategoryStrength = {
  category: string
  averageScore: number
  attemptCount: number
}

export type RecentActivityItem = {
  attemptId: string
  category: string
  scorePercentage: number
  completedAt: string
}

export type DashboardSummary = {
  currentStreakDays: number
  activitySparkline: number[]
  scoreOverTime: ScorePoint[]
  categoryStrength: CategoryStrength[]
  totalQuestionsAnswered: number
  averageScore: number | null
  recentActivity: RecentActivityItem[]
}

// Mirrors the API's DashboardRange enum (bound case-insensitively by name on the query string).
export type DashboardRange = 'week' | 'month' | 'all'

export async function fetchDashboardSummary(range: DashboardRange): Promise<DashboardSummary> {
  const { data } = await apiClient.get<DashboardSummary>('/api/dashboard', { params: { range } })
  return data
}
