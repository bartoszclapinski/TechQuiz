import { useQuery } from '@tanstack/react-query'
import { fetchAchievements } from './api'
import { achievementsKey } from './query-keys'

// One read fans out into the whole achievements section, so it's a single TanStack query — cached and
// self-refetching, invalidated by the quiz-complete and review-grade success handlers.
export function useAchievements() {
  return useQuery({
    queryKey: achievementsKey,
    queryFn: fetchAchievements,
  })
}
