import { useInfiniteQuery } from '@tanstack/react-query'
import { fetchHistory, HISTORY_PAGE_SIZE, type HistoryItem } from './api'
import { historyKey, type HistoryFilters } from './query-keys'

// History is paged with TanStack's `useInfiniteQuery` — a query that holds *a list of pages* in one
// cache entry, instead of one query per page. The flow:
//
//  - `initialPageParam` is the first page number we fetch (1).
//  - `queryFn` receives the current `pageParam` and returns that page's items.
//  - `getNextPageParam(lastPage, allPages)` decides the *next* page number, or `undefined` to stop.
//    We stop when the last page came back shorter than a full page: a short page is the natural
//    "no more rows" sentinel, so we never need a separate total-count round-trip. Otherwise the next
//    page number is `allPages.length + 1`.
//
// The page reads `data.pages` (an array of arrays) and flattens it; `fetchNextPage()` appends the
// next page, `hasNextPage` reflects whether `getNextPageParam` returned a number, and
// `isFetchingNextPage` drives the "Load more" button's loading state.
export function useHistory(filters: HistoryFilters) {
  return useInfiniteQuery({
    queryKey: historyKey(filters),
    initialPageParam: 1,
    queryFn: ({ pageParam }) =>
      fetchHistory({
        category: filters.category,
        sortBy: filters.sortBy,
        descending: filters.descending,
        page: pageParam,
        pageSize: HISTORY_PAGE_SIZE,
      }),
    getNextPageParam: (lastPage: HistoryItem[], allPages: HistoryItem[][]) =>
      lastPage.length < HISTORY_PAGE_SIZE ? undefined : allPages.length + 1,
  })
}
