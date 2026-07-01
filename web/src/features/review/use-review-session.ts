import { useQuery } from '@tanstack/react-query'
import { fetchReviewSession, type ReviewSessionDetail } from './api'
import { reviewSessionKey } from './query-keys'

// One past session's graded detail, read on demand when the user opens a history row. Sessions are
// immutable once completed, so the cache never goes stale.
export function useReviewSession(id: string) {
  return useQuery<ReviewSessionDetail>({
    queryKey: reviewSessionKey(id),
    queryFn: () => fetchReviewSession(id),
  })
}
