import { useMutation, useQueryClient } from '@tanstack/react-query'
import { publishPooledQuestion } from './api'
import { pooledQuestionsKey } from './query-keys'

// Publishing a draft is a write, so it's a mutation. On success we invalidate the pool browse
// query so the newly published question appears without a manual refresh. The page owns the
// success/error toast; the hook stays thin.
export function usePublishQuestion() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => publishPooledQuestion(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: pooledQuestionsKey() }),
  })
}
