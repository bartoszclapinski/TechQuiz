import { useMutation } from '@tanstack/react-query'
import { getCodeFeedback } from './api'

// Asking for AI feedback is a write/action, not a read: it spends a provider call (the user's own
// tokens) and returns prose we don't cache against any entity — so it's a mutation, like Run and
// Submit, not a query. Unlike useGradeChallenge it also needs the chosen provider, so the mutate
// variables carry both the source and the provider name. The page reads `data` (the prose),
// `isPending` (loading), and `isError`/`error` (the 409 "no key" path) to render its states.
export function useCodeFeedback(challengeId: string) {
  return useMutation({
    mutationFn: (vars: { sourceCode: string; provider: string }) =>
      getCodeFeedback(challengeId, vars.sourceCode, vars.provider),
  })
}
