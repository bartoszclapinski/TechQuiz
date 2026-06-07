import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'
import { submitAnswer } from './api'

type SubmitVariables = { attemptId: string; questionId: string; selectedOptionId: string | null }

// Answers save as the user picks them — local state already reflects the choice optimistically.
// The backend upserts per question, so re-firing when the user changes their mind before advancing
// is idempotent. A successful save needs no UI; only errors surface a toast.
export function useSubmitAnswer() {
  return useMutation({
    mutationFn: ({ attemptId, questionId, selectedOptionId }: SubmitVariables) =>
      submitAnswer(attemptId, questionId, selectedOptionId),
    onError: () => {
      toast.error('Could not save your answer.')
    },
  })
}
