import { useQuery } from '@tanstack/react-query'
import { fetchConfiguredProviders } from './api'
import { aiKeysKey } from './query-keys'

export function useConfiguredProviders() {
  return useQuery({ queryKey: aiKeysKey(), queryFn: fetchConfiguredProviders })
}
