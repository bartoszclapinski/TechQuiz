import { useQuery } from '@tanstack/react-query'
import { fetchTracks } from './api'
import { categoriesKey } from './query-keys'

// The catalogue endpoint returns tracks with nested subcategories (ADR-023).
export function useTracks() {
  return useQuery({ queryKey: categoriesKey(), queryFn: fetchTracks })
}
