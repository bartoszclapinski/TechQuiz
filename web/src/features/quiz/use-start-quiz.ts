import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { startQuiz } from './api'
import { quizSessionKey } from './query-keys'

// Starting a quiz is a write (it creates an attempt), so it's a mutation rather than a query.
// On success we seed the React Query cache with the returned session so QuizPage can read the
// questions without a second round-trip, then navigate into the runner.
export function useStartQuiz() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: startQuiz,
    onSuccess: (session) => {
      queryClient.setQueryData(quizSessionKey(session.attemptId), session)
      navigate(`/quiz/${session.attemptId}`)
    },
    onError: () => {
      toast.error('Could not start the quiz. Please try again.')
    },
  })
}
