import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { removeAiKey } from './api'
import { aiKeysKey } from './query-keys'

// Removing a key invalidates the configured-providers query so the row flips back to
// "Not configured". A removal failure is unexpected (no validation path), so a toast is the
// right surface here.
export function useRemoveAiKey() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (provider: string) => removeAiKey(provider),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: aiKeysKey() }),
    onError: () => toast.error('Could not remove the key. Please try again.'),
  })
}
