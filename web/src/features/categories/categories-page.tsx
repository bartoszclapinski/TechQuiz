import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTracks } from './use-categories'
import type { Category, Track } from './api'
import { useStartQuiz } from '../quiz/use-start-quiz'

// Fluid card grid — auto-fill so cards keep a comfortable min width and the row fills wide screens
// (matches the handoff's `repeat(auto-fit, minmax(300px, 1fr))`).
const CARD_GRID = 'grid gap-3.5 [grid-template-columns:repeat(auto-fill,minmax(300px,1fr))]'

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
            className="rounded-pill border border-strong px-4 py-1.5 text-[15px] font-medium transition-colors hover:bg-elevated"
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
        <div className={CARD_GRID}>
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

  const topicTotal = tracks.reduce((sum, t) => sum + t.categories.length, 0)

  return (
    <PageShell
      title="Pick a category"
      subtitle={`${tracks.length} tracks · ${topicTotal} topics — start where you feel strong, or challenge a weak spot.`}
    >
      <div className={CARD_GRID}>
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
    <main className="mx-auto max-w-[1560px] px-4 py-8 sm:px-6 lg:px-12">
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
          <h1 className="mb-1.5 font-display text-[clamp(28px,3vw,40px)] font-extrabold leading-[1.05] tracking-[-0.02em]">
            {title}
          </h1>
          {subtitle ? <p className="text-[16px] text-secondary">{subtitle}</p> : null}
        </div>
      ) : null}
      {children}
    </main>
  )
}

// Gradient monogram tile shared by track + active category cards.
function IconTile({ code, muted = false }: { code: string; muted?: boolean }) {
  return (
    <div
      className={`flex h-[46px] min-w-[46px] items-center justify-center rounded-[13px] px-2 font-display text-[16px] font-extrabold ${
        muted ? 'bg-elevated text-muted' : 'bg-brand text-brandfg'
      }`}
    >
      {code}
    </div>
  )
}

function Pill({ children, muted = false }: { children: React.ReactNode; muted?: boolean }) {
  return (
    <span
      className={`rounded-pill bg-elevated px-2.5 py-1 font-mono text-[12px] font-semibold ${
        muted ? 'text-muted' : 'text-secondary'
      }`}
    >
      {children}
    </span>
  )
}

function TrackTile({ track, onOpen }: { track: Track; onOpen: () => void }) {
  const topicCount = track.categories.length
  const questionCount = track.categories.reduce((sum, c) => sum + c.questionCount, 0)

  return (
    <button
      type="button"
      onClick={onOpen}
      className="block rounded-[20px] border border-default bg-surface p-6 text-left transition-colors hover:border-strong"
    >
      <div className="mb-4 flex items-start justify-between">
        <IconTile code={track.iconCode} />
        <Pill>{topicCount} topics</Pill>
      </div>
      <h3 className="mb-1 font-display text-[19px] font-bold">{track.name}</h3>
      <p className="mb-4 text-[14px] leading-[1.5] text-secondary">{track.description}</p>
      <p className="font-mono text-[12px] text-muted">{questionCount} questions →</p>
    </button>
  )
}

function PracticalChallengesTile({ onOpen }: { onOpen: () => void }) {
  return (
    <button
      type="button"
      onClick={onOpen}
      className="block rounded-[20px] border border-dashed border-strong bg-base p-6 text-left opacity-90 transition-colors hover:bg-elevated hover:opacity-100"
    >
      <div className="mb-4 flex items-start justify-between">
        <IconTile code={'</>'} muted />
        <Pill muted>hands-on</Pill>
      </div>
      <h3 className="mb-1 font-display text-[19px] font-bold">Practical Challenges</h3>
      <p className="mb-4 text-[14px] leading-[1.5] text-secondary">
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
      <div className="rounded-[20px] border border-dashed border-strong bg-base p-6 opacity-70">
        <div className="mb-4 flex items-start justify-between">
          <IconTile code={category.iconCode} muted />
          <Pill muted>Soon</Pill>
        </div>
        <h3 className="mb-1 font-display text-[19px] font-bold text-secondary">{category.name}</h3>
        <p className="mb-4 text-[14px] leading-[1.5] text-muted">{category.description}</p>
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
      className="block rounded-[20px] border border-default bg-surface p-6 text-left transition-colors hover:border-strong disabled:cursor-not-allowed disabled:opacity-70"
    >
      <div className="mb-4 flex items-start justify-between">
        <IconTile code={category.iconCode} />
        <Pill>{starting ? 'Starting…' : `${category.questionCount} q`}</Pill>
      </div>
      <h3 className="mb-1 font-display text-[19px] font-bold">{category.name}</h3>
      <p className="mb-4 text-[14px] leading-[1.5] text-secondary">{category.description}</p>
      {score > 0 ? (
        <div className="flex items-center gap-2.5">
          <div className="h-[7px] flex-1 overflow-hidden rounded-pill bg-track">
            <div className="h-full rounded-pill bg-brand" style={{ width: `${score}%` }} />
          </div>
          <span className="font-mono text-[13px] font-semibold text-amber-text">{score}%</span>
        </div>
      ) : (
        <p className="font-mono text-[12px] text-muted">Not started</p>
      )}
    </button>
  )
}
