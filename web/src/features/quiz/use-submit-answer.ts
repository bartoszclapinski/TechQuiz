import { useMutation } from '@tanstack/react-query'
import { submitAnswer } from './api'

type SubmitVariables = { attemptId: string; questionId: string; selectedOptionId: string | null }

// Answers save as the user picks them — local state already reflects the choice optimistically.
// The backend upserts per question, so re-firing when the user changes their mind before advancing
// is idempotent. Error handling (rollback + toast) lives in the runner via mutateAsync, since it
// needs to undo the optimistic selection for the specific question that failed.
export function useSubmitAnswer() {
  return useMutation({
    mutationFn: ({ attemptId, questionId, selectedOptionId }: SubmitVariables) =>
      submitAnswer(attemptId, questionId, selectedOptionId),
  })
}
