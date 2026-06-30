import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { gradeReview, type ReviewAnswer, type ReviewGradeResult } from './api'

// Submitting a review is an action that spends a call and returns the graded results — a mutation,
// not a query (it caches nothing; the queue lives in the daily-review query). The caller keeps the
// returned results in component state to render the summary.
export function useGradeReview() {
  return useMutation<ReviewGradeResult[], unknown, ReviewAnswer[]>({
    mutationFn: (answers) => gradeReview(answers),
    onError: () => {
      toast.error('Could not grade your review. Please try again.')
    },
  })
}
