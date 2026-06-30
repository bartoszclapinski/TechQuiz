import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { gradeReview, type ReviewAnswer, type ReviewGradeResult } from './api'
import { dailyReviewKey, reviewStatsKey } from './query-keys'

// Submitting a review is an action that spends a call and returns the graded results — a mutation,
// not a query (it caches nothing; the queue lives in the daily-review query). The caller keeps the
// returned results in component state to render the summary.
//
// On success we invalidate the queue and stats keys: the session is now persisted (ADR-021), so a
// correct answer drops its question from the queue and the streak / reviewed-today flag advance. The
// running summary is unaffected — it renders from frozen session state, not these queries.
export function useGradeReview() {
  const queryClient = useQueryClient()

  return useMutation<ReviewGradeResult[], unknown, ReviewAnswer[]>({
    mutationFn: (answers) => gradeReview(answers),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: dailyReviewKey })
      void queryClient.invalidateQueries({ queryKey: reviewStatsKey })
    },
    onError: () => {
      toast.error('Could not grade your review. Please try again.')
    },
  })
}
