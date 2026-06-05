import { useQuery } from '@tanstack/react-query'
import { fetchCategories } from './api'
import { categoriesKey } from './query-keys'

export function useCategories() {
  return useQuery({ queryKey: categoriesKey(), queryFn: fetchCategories })
}
