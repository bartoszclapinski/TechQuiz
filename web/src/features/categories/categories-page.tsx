import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTracks } from './use-categories'
import type { Category, Track } from './api'
import { useStartQuiz } from '../quiz/use-start-quiz'

export function CategoriesPage() {
  const { data: tracks, isLoading, isError, refetch } = useTracks()
  const navigate = useNavigate()
  const start = useStartQuiz()
  const startingId = start.isPending ? start.variables?.id : undefined

  // Master/detail without a route change: pick a track tile to reveal its subcategories,
  // "back" clears the selection. Deep-linking to a track isn't needed for the catalogue.
  const [selectedTrackId, setSelectedTrackId] = useState<string | null>(null)
  const selectedTrack = tracks?.find((t) => t.id === selectedTrackId) ?? null

  if (isLoading) {
    return (
      <PageShell>
        <p className="text-[15px] text-secondary">Loading catalogue…</p>
      </PageShell>
    )
  }

  if (isError || !tracks) {
    return (
      <PageShell>
        <div className="text-[15px] text-secondary">
          <p className="mb-2 text-danger">Could not load the catalogue.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-md border border-strong px-3 py-1.5 text-[15px] font-medium transition-colors hover:bg-elevated"
          >
            Retry
          </button>
        </div>
      </PageShell>
    )
  }

  if (selectedTrack) {
    return (
      <PageShell
        title={selectedTrack.name}
        subtitle={selectedTrack.description}
        onBack={() => setSelectedTrackId(null)}
      >
        <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2 lg:grid-cols-3">
          {selectedTrack.categories.map((category) => (
            <CategoryCard
              key={category.id}
              category={category}
              starting={startingId === category.id}
              disabled={start.isPending}
              onStart={() => start.mutate({ id: category.id, name: category.name })}
            />
          ))}
        </div>
      </PageShell>
    )
  }

  return (
    <PageShell
      title="Categories"
      subtitle="Pick a track, then choose a topic to test your knowledge."
    >
      <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2 lg:grid-cols-3">
        {tracks.map((track) => (
          <TrackTile key={track.id} track={track} onOpen={() => setSelectedTrackId(track.id)} />
        ))}
        <PracticalChallengesTile onOpen={() => navigate('/challenges')} />
      </div>
    </PageShell>
  )
}

function PageShell({
  children,
  title,
  subtitle,
  onBack,
}: {
  children: React.ReactNode
  title?: string
  subtitle?: string
  onBack?: () => void
}) {
  return (
    <main className="mx-auto max-w-6xl px-6 py-8 sm:px-9">
      {title ? (
        <div className="mb-7">
          {onBack ? (
            <button
              type="button"
              onClick={onBack}
              className="mb-3 inline-flex items-center gap-1.5 text-[14px] font-medium text-secondary transition-colors hover:text-primary"
            >
              <span aria-hidden="true">←</span> All tracks
            </button>
          ) : null}
          <h1 className="mb-1 text-2xl font-semibold tracking-tight">{title}</h1>
          {subtitle ? <p className="text-[14px] text-secondary">{subtitle}</p> : null}
        </div>
      ) : null}
      {children}
    </main>
  )
}

function TrackTile({ track, onOpen }: { track: Track; onOpen: () => void }) {
  const topicCount = track.categories.length
  const questionCount = track.categories.reduce((sum, c) => sum + c.questionCount, 0)

  return (
    <button
      type="button"
      onClick={onOpen}
      className="block rounded-[10px] border border-default bg-surface p-3.5 text-left transition-colors hover:border-strong"
    >
      <div className="mb-2.5 flex items-start justify-between">
        <div className="flex h-8 min-w-8 items-center justify-center rounded-md bg-accent-bg px-1.5 font-mono text-[13px] font-semibold text-accent-text">
          {track.iconCode}
        </div>
        <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[12px] text-secondary">
          {topicCount} topics
        </span>
      </div>
      <p className="mb-0.5 text-[14px] font-semibold">{track.name}</p>
      <p className="mb-2.5 text-[13px] leading-snug text-secondary">{track.description}</p>
      <p className="font-mono text-[12px] text-muted">{questionCount} questions</p>
    </button>
  )
}

function PracticalChallengesTile({ onOpen }: { onOpen: () => void }) {
  return (
    <button
      type="button"
      onClick={onOpen}
      className="block rounded-[10px] border border-dashed border-strong bg-base p-3.5 text-left transition-colors hover:bg-elevated"
    >
      <div className="mb-2.5 flex items-start justify-between">
        <div className="flex h-8 min-w-8 items-center justify-center rounded-md bg-elevated px-1.5 font-mono text-[13px] font-semibold text-secondary">
          {'</>'}
        </div>
        <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[12px] text-secondary">
          hands-on
        </span>
      </div>
      <p className="mb-0.5 text-[14px] font-semibold">Practical Challenges</p>
      <p className="mb-2.5 text-[13px] leading-snug text-secondary">
        Write and run code against automated tests instead of picking an answer.
      </p>
      <p className="font-mono text-[12px] text-muted">Open editor →</p>
    </button>
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
          <div className="flex h-8 min-w-8 items-center justify-center rounded-md bg-elevated px-1.5 font-mono text-[13px] font-semibold text-muted">
            {category.iconCode}
          </div>
          <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[11px] text-secondary">
            Coming soon
          </span>
        </div>
        <p className="mb-0.5 text-[14px] font-semibold text-primary">{category.name}</p>
        <p className="mb-2.5 text-[13px] leading-snug text-muted">{category.description}</p>
        <p className="font-mono text-[12px] text-muted">Not started</p>
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
        <div className="flex h-8 min-w-8 items-center justify-center rounded-md bg-accent-bg px-1.5 font-mono text-[13px] font-semibold text-accent-text">
          {category.iconCode}
        </div>
        <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[12px] text-secondary">
          {starting ? 'Starting…' : `${category.questionCount} q`}
        </span>
      </div>
      <p className="mb-0.5 text-[14px] font-semibold">{category.name}</p>
      <p className="mb-2.5 text-[13px] leading-snug text-secondary">{category.description}</p>
      {score > 0 ? (
        <div className="flex items-center gap-2">
          <div className="h-[3px] flex-1 overflow-hidden rounded-full bg-elevated">
            <div className="h-full rounded-full bg-accent" style={{ width: `${score}%` }} />
          </div>
          <span className="font-mono text-[12px] font-semibold text-accent-text">{score}%</span>
        </div>
      ) : (
        <p className="font-mono text-[12px] text-muted">Not started</p>
      )}
    </button>
  )
}
