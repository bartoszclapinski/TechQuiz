import type { HistorySortBy } from './api'

// The filter + sort selection is folded into the key so each distinct view caches independently;
// the page param is NOT part of the key — useInfiniteQuery manages pages within a single cache entry.
export type HistoryFilters = {
  category: string | null
  sortBy: HistorySortBy
  descending: boolean
}

export const historyKey = (filters: HistoryFilters) =>
  ['history', filters.category, filters.sortBy, filters.descending] as const
