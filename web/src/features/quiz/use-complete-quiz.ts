import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { achievementsKey } from '../achievements/query-keys'
import { completeQuiz } from './api'

// Completing scores the attempt server-side; on success we move to the result screen. The result
// page (iteration 1.7) fetches the breakdown via GET /result, so we don't need the response here.
export function useCompleteQuiz() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (attemptId: string) => completeQuiz(attemptId),
    onSuccess: (_data, attemptId) => {
      // A finished quiz can cross a badge threshold (quiz count, questions answered, a perfect score,
      // a streak day), so refresh the catalogue for the next Dashboard visit.
      void queryClient.invalidateQueries({ queryKey: achievementsKey })
      navigate(`/result/${attemptId}`)
    },
    onError: () => {
      toast.error('Could not submit the quiz. Please try again.')
    },
  })
}
