import { useMutation } from '@tanstack/react-query'
import { generateQuestions } from './api'

// Generation is a write that spends the user's provider tokens, so it's a mutation, not a query —
// it must fire only on explicit submit, never on render/refetch. The drafts are ephemeral (not
// persisted until 3.5), so there's no cache to seed; the page holds the result in local state.
export function useGenerateQuestions() {
  return useMutation({ mutationFn: generateQuestions })
}
