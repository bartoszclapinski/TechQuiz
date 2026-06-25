import { useMutation, useQueryClient } from '@tanstack/react-query'
import { setAiKey } from './api'
import { aiKeysKey } from './query-keys'

type SetKeyVariables = { provider: string; apiKey: string }

// Storing a key is a write, so it's a mutation. On success we invalidate the configured-providers
// query so the list refetches and the row flips to "Configured". The page owns success/error
// feedback (inline) — the hook stays thin and never holds the key beyond the request.
export function useSetAiKey() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ provider, apiKey }: SetKeyVariables) => setAiKey(provider, apiKey),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: aiKeysKey() }),
  })
}
