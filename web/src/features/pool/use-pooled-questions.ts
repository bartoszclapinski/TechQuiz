import { useQuery } from '@tanstack/react-query'
import { fetchPooledQuestions } from './api'
import { pooledQuestionsKey } from './query-keys'

// Browsing the pool is a read, so it's a query — it caches and refetches on its own. The publish
// mutation invalidates pooledQuestionsKey() so a freshly published draft shows up here.
export function usePooledQuestions() {
  return useQuery({ queryKey: pooledQuestionsKey(), queryFn: fetchPooledQuestions })
}
