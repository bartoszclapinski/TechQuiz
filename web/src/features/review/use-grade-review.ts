import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { achievementsKey } from '../achievements/query-keys'
import { gradeReview, type ReviewAnswer, type ReviewGradeResult } from './api'
import { dailyReviewKey, reviewSessionsKey, reviewStatsKey } from './query-keys'

// Submitting a review is an action that spends a call and returns the graded results — a mutation,
// not a query (it caches nothing; the queue lives in the daily-review query). The caller keeps the
// returned results in component state to render the summary.
//
// On success we invalidate the queue, stats, and sessions keys: the session is now persisted (ADR-021),
// so a correct answer drops its question from the queue, the streak / reviewed-today flag advance, and
// the just-completed session must appear in the hub's history. The running summary is unaffected — it
// renders from frozen session state, not these queries.
export function useGradeReview() {
  const queryClient = useQueryClient()

  return useMutation<ReviewGradeResult[], unknown, ReviewAnswer[]>({
    mutationFn: (answers) => gradeReview(answers),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: dailyReviewKey })
      void queryClient.invalidateQueries({ queryKey: reviewStatsKey })
      void queryClient.invalidateQueries({ queryKey: reviewSessionsKey })
      // Review answers feed the review-based badges (first review, 50 correct, streak days).
      void queryClient.invalidateQueries({ queryKey: achievementsKey })
    },
    onError: () => {
      toast.error('Could not grade your review. Please try again.')
    },
  })
}
