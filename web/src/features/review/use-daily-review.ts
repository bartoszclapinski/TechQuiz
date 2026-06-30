import { useQuery } from '@tanstack/react-query'
import { fetchDailyReview, type ReviewQuestion } from './api'
import { dailyReviewKey } from './query-keys'

// The daily-review queue is one read shared by the Dashboard card (which shows its length) and the
// `/review` runner (which plays it). A single TanStack query under one key serves both: the card
// warms the cache, so entering the review needs no extra round-trip.
export function useDailyReview() {
  return useQuery<ReviewQuestion[]>({
    queryKey: dailyReviewKey,
    queryFn: () => fetchDailyReview(),
  })
}
