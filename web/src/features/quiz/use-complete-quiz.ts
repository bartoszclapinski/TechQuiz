import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { completeQuiz } from './api'

// Completing scores the attempt server-side; on success we move to the result screen. The result
// page (iteration 1.7) fetches the breakdown via GET /result, so we don't need the response here.
export function useCompleteQuiz() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: (attemptId: string) => completeQuiz(attemptId),
    onSuccess: (_data, attemptId) => {
      navigate(`/result/${attemptId}`)
    },
    onError: () => {
      toast.error('Could not submit the quiz. Please try again.')
    },
  })
}
