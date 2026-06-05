import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { startQuiz } from './api'
import { quizSessionKey } from './query-keys'
import type { QuizRunnerSession } from './types'

type StartVariables = { id: string; name: string }

// Starting a quiz is a write (it creates an attempt), so it's a mutation rather than a query.
// On success we seed the React Query cache with the returned session — augmented with the clicked
// category's name so the mini-header can render it — then navigate into the runner. This avoids a
// second round-trip and a backend field that the API doesn't currently expose.
export function useStartQuiz() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: ({ id }: StartVariables) => startQuiz(id),
    onSuccess: (session, { name }) => {
      const runnerSession: QuizRunnerSession = { ...session, categoryName: name }
      queryClient.setQueryData(quizSessionKey(session.attemptId), runnerSession)
      navigate(`/quiz/${session.attemptId}`)
    },
    onError: () => {
      toast.error('Could not start the quiz. Please try again.')
    },
  })
}
