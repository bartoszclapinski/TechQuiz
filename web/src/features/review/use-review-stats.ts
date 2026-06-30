import { useQuery } from '@tanstack/react-query'
import { fetchReviewStats, type ReviewStats } from './api'
import { reviewStatsKey } from './query-keys'

// Review-specific aggregates for the Dashboard: streaks, accuracy, totals, and the reviewed-today
// flag the banner uses to switch to its "done for today" state. Grading invalidates this key, so the
// numbers refresh as soon as the user finishes a session.
export function useReviewStats() {
  return useQuery<ReviewStats>({
    queryKey: reviewStatsKey,
    queryFn: () => fetchReviewStats(),
  })
}
