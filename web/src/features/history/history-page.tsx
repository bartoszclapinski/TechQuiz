import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useCategories } from '../categories/use-categories'
import { useHistory } from './use-history'
import type { HistoryItem, HistorySortBy } from './api'
import type { HistoryFilters } from './query-keys'

export function HistoryPage() {
  const [category, setCategory] = useState<string | null>(null)
  const [sortBy, setSortBy] = useState<HistorySortBy>('date')
  const [descending, setDescending] = useState(true)

  const filters: HistoryFilters = { category, sortBy, descending }
  const {
    data,
    isLoading,
    isError,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useHistory(filters)

  const { data: categories } = useCategories()
  const items = data?.pages.flat() ?? []

  // Clicking the active field flips direction; clicking the other field switches to it (newest /
  // highest first by default).
  function selectSort(field: HistorySortBy) {
    if (field === sortBy) {
      setDescending((d) => !d)
    } else {
      setSortBy(field)
      setDescending(true)
    }
  }

  return (
    <main className="mx-auto max-w-3xl px-6 py-8 sm:px-8">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">History</h1>
          <p className="mt-1 text-[13px] text-secondary">Every quiz you’ve completed.</p>
        </div>
        <div className="flex flex-wrap items-center gap-2 self-start">
          <select
            value={category ?? ''}
            onChange={(event) => setCategory(event.target.value || null)}
            aria-label="Filter by category"
            className="rounded-lg border border-default bg-surface px-3 py-1.5 text-[13px] font-medium text-primary"
          >
            <option value="">All categories</option>
            {categories?.map((c) => (
              <option key={c.id} value={c.name}>
                {c.name}
              </option>
            ))}
          </select>
          <SortControl sortBy={sortBy} descending={descending} onSelect={selectSort} />
        </div>
      </div>

      {isLoading ? (
        <p className="text-sm text-secondary">Loading your history…</p>
      ) : isError ? (
        <p className="text-sm text-danger">Couldn’t load your history.</p>
      ) : items.length === 0 ? (
        <EmptyHistory hasFilter={category !== null} />
      ) : (
        <>
          <div className="flex flex-col rounded-xl border border-default bg-surface px-[18px]">
            {items.map((item) => (
              <HistoryRow key={item.attemptId} item={item} />
            ))}
          </div>
          {hasNextPage ? (
            <div className="mt-4 flex justify-center">
              <button
                type="button"
                onClick={() => void fetchNextPage()}
                disabled={isFetchingNextPage}
                className="rounded-lg border border-default bg-surface px-4 py-2 text-[13px] font-medium text-primary hover:bg-elevated disabled:opacity-60"
              >
                {isFetchingNextPage ? 'Loading…' : 'Load more'}
              </button>
            </div>
          ) : null}
        </>
      )}
    </main>
  )
}

function HistoryRow({ item }: { item: HistoryItem }) {
  return (
    <Link
      to={`/result/${item.attemptId}`}
      className="flex items-center gap-2.5 border-b border-default py-3 last:border-b-0 hover:opacity-80"
    >
      <div className="flex h-[26px] w-[26px] items-center justify-center rounded-md bg-accent-bg font-mono text-[10px] font-semibold text-accent-text">
        {categoryBadge(item.category)}
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-[13px] font-medium text-primary">{item.category}</p>
        <p className="font-mono text-[10px] text-secondary">{formatDate(item.completedAt)}</p>
      </div>
      <span className={`font-mono text-[13px] font-bold ${scoreColor(item.scorePercentage)}`}>
        {Math.round(item.scorePercentage)}%
      </span>
    </Link>
  )
}

function SortControl({
  sortBy,
  descending,
  onSelect,
}: {
  sortBy: HistorySortBy
  descending: boolean
  onSelect: (field: HistorySortBy) => void
}) {
  const fields: { value: HistorySortBy; label: string }[] = [
    { value: 'date', label: 'Date' },
    { value: 'score', label: 'Score' },
  ]
  return (
    <div className="flex gap-0.5 rounded-lg border border-default bg-base p-[3px]">
      {fields.map((field) => {
        const active = field.value === sortBy
        return (
          <button
            key={field.value}
            type="button"
            aria-pressed={active}
            onClick={() => onSelect(field.value)}
            className={`flex items-center gap-1 rounded-md px-3 py-1 font-mono text-xs font-medium transition-colors ${
              active ? 'bg-elevated text-primary' : 'text-secondary hover:text-primary'
            }`}
          >
            {field.label}
            {active ? <span aria-hidden="true">{descending ? '↓' : '↑'}</span> : null}
          </button>
        )
      })}
    </div>
  )
}

function EmptyHistory({ hasFilter }: { hasFilter: boolean }) {
  if (hasFilter) {
    return <p className="text-sm text-secondary">No attempts in this category yet.</p>
  }
  return (
    <div className="rounded-xl border border-default bg-surface px-6 py-10 text-center">
      <h3 className="mb-1.5 text-[15px] font-semibold text-primary">No attempts yet</h3>
      <p className="mb-4 text-xs text-secondary">
        Complete a quiz and it’ll show up here.
      </p>
      <Link
        to="/categories"
        className="inline-flex items-center gap-1.5 rounded-lg bg-accent px-4 py-2 text-[13px] font-medium text-white hover:opacity-90"
      >
        Browse categories
      </Link>
    </div>
  )
}

function scoreColor(score: number): string {
  if (score >= 80) return 'text-success'
  if (score >= 70) return 'text-accent-text'
  return 'text-warning'
}

function categoryBadge(name: string): string {
  const first = name.trim().split(/\s+/)[0] ?? ''
  return (first.length <= 4 ? first : first.slice(0, 3)).toUpperCase()
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
