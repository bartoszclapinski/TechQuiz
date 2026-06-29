import { useMutation } from '@tanstack/react-query'
import { gradeChallenge } from './api'

// Submitting for grading runs the submission against the hidden tests — a one-shot action,
// so it's a mutation. There's no cached entity to invalidate (the verdict isn't stored), so
// the page just reads `data` (the grade result) and `isPending` to render the verdict.
export function useGradeChallenge(challengeId: string) {
  return useMutation({
    mutationFn: (sourceCode: string) => gradeChallenge(challengeId, sourceCode),
  })
}
