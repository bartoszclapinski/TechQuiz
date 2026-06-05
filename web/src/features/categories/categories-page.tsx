import { useCategories } from './use-categories'
import type { Category } from './api'
import { useStartQuiz } from '../quiz/use-start-quiz'

export function CategoriesPage() {
  const { data: categories, isLoading, isError, refetch } = useCategories()
  const start = useStartQuiz()
  const startingId = start.isPending ? start.variables : undefined

  return (
    <main className="mx-auto max-w-6xl px-6 py-8 sm:px-9">
      <div className="mb-7">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Categories</h1>
        <p className="text-[13px] text-secondary">Pick a topic to start testing your knowledge.</p>
      </div>

      {isLoading ? (
        <p className="text-sm text-secondary">Loading categories…</p>
      ) : isError ? (
        <div className="text-sm text-secondary">
          <p className="mb-2 text-danger">Could not load categories.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-md border border-strong px-3 py-1.5 text-sm font-medium transition-colors hover:bg-elevated"
          >
            Retry
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2 lg:grid-cols-3">
          {categories?.map((category) => (
            <CategoryCard
              key={category.id}
              category={category}
              starting={startingId === category.id}
              disabled={start.isPending}
              onStart={() => start.mutate(category.id)}
            />
          ))}
        </div>
      )}
    </main>
  )
}

function CategoryCard({
  category,
  starting,
  disabled,
  onStart,
}: {
  category: Category
  starting: boolean
  disabled: boolean
  onStart: () => void
}) {
  const available = category.questionCount > 0
  const score = Math.round(category.userBestScore)

  if (!available) {
    return (
      <div className="rounded-[10px] border border-default bg-base p-3.5 opacity-60">
        <div className="mb-2.5 flex items-start justify-between">
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-elevated font-mono text-[11px] font-semibold text-muted">
            {category.iconCode}
          </div>
          <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[9px] text-secondary">
            Coming soon
          </span>
        </div>
        <p className="mb-0.5 text-[13px] font-semibold text-primary">{category.name}</p>
        <p className="mb-2.5 text-[11px] leading-snug text-muted">{category.description}</p>
        <p className="font-mono text-[10px] text-muted">Not started</p>
      </div>
    )
  }

  return (
    <button
      type="button"
      onClick={onStart}
      disabled={disabled}
      aria-busy={starting}
      className="block rounded-[10px] border border-default bg-surface p-3.5 text-left transition-colors hover:border-strong disabled:cursor-not-allowed disabled:opacity-70"
    >
      <div className="mb-2.5 flex items-start justify-between">
        <div className="flex h-8 w-8 items-center justify-center rounded-md bg-accent-bg font-mono text-[11px] font-semibold text-accent-text">
          {category.iconCode}
        </div>
        <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[10px] text-secondary">
          {starting ? 'Starting…' : `${category.questionCount} q`}
        </span>
      </div>
      <p className="mb-0.5 text-[13px] font-semibold">{category.name}</p>
      <p className="mb-2.5 text-[11px] leading-snug text-secondary">{category.description}</p>
      <div className="flex items-center gap-2">
        <div className="h-[3px] flex-1 overflow-hidden rounded-full bg-elevated">
          <div className="h-full rounded-full bg-accent" style={{ width: `${score}%` }} />
        </div>
        <span className="font-mono text-[10px] font-semibold text-accent-text">{score}%</span>
      </div>
    </button>
  )
}
