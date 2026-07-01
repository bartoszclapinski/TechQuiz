import { useQuery } from '@tanstack/react-query'
import { fetchReviewSessions, type ReviewSessionSummary } from './api'
import { reviewSessionsKey } from './query-keys'

// The user's past review sessions, newest first (the API orders them). Powers the hub's history list;
// grading invalidates this key so a freshly completed session appears without a manual refresh.
export function useReviewSessions() {
  return useQuery<ReviewSessionSummary[]>({
    queryKey: reviewSessionsKey,
    queryFn: () => fetchReviewSessions(),
  })
}
