import { useQuery } from '@tanstack/react-query'
import { fetchQuizResult } from './api'
import { quizResultKey } from './query-keys'

// A completed attempt's result never changes, so we disable refetching and treat it as always-fresh.
export function useQuizResult(attemptId: string) {
  return useQuery({
    queryKey: quizResultKey(attemptId),
    queryFn: () => fetchQuizResult(attemptId),
    staleTime: Infinity,
    retry: 1,
  })
}
