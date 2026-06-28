import { useQuery } from '@tanstack/react-query'
import { fetchCodeChallenges } from './api'
import { codeChallengesKey } from './query-keys'

// Listing challenges is a read, so it's a query — cached and self-refetching. The catalog is
// static seed data today, so this rarely changes during a session.
export function useCodeChallenges() {
  return useQuery({ queryKey: codeChallengesKey(), queryFn: fetchCodeChallenges })
}
