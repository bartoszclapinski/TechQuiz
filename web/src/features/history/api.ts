import { apiClient } from '../../lib/api-client'

// Mirrors the API's HistoryItemDto — one completed, scored attempt with its category name.
export type HistoryItem = {
  attemptId: string
  category: string
  scorePercentage: number
  completedAt: string
}

// Mirrors the API's HistorySortField enum (bound case-insensitively by name on the query string).
export type HistorySortBy = 'date' | 'score'

export type HistoryQuery = {
  category: string | null
  sortBy: HistorySortBy
  descending: boolean
  page: number
  pageSize: number
}

export const HISTORY_PAGE_SIZE = 20

export async function fetchHistory(query: HistoryQuery): Promise<HistoryItem[]> {
  const { category, sortBy, descending, page, pageSize } = query
  const { data } = await apiClient.get<HistoryItem[]>('/api/history', {
    // category is omitted when null so the server returns every category.
    params: { ...(category ? { category } : {}), sortBy, descending, page, pageSize },
  })
  return data
}
