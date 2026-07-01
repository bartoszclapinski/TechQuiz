import { Link } from 'react-router-dom'
import type { ReviewSessionSummary, ReviewStats } from './api'
import { useDailyReview } from './use-daily-review'
import { useReviewStats } from './use-review-stats'
import { useReviewSessions } from './use-review-sessions'

// The daily-review hub at /review: stats, today's action (start or done), and a history of past
// sessions. The runner lives at /review/run; each history row opens /review/sessions/:id.
export function ReviewHubPage() {
  const stats = useReviewStats()
  const sessions = useReviewSessions()

  return (
    <main className="mx-auto max-w-[800px] px-6 py-8 sm:px-9">
      <div className="mb-6">
        <p className="mb-1.5 font-mono text-[11px] uppercase tracking-[0.1em] text-secondary">Spaced repetition</p>
        <h1 className="text-2xl font-semibold leading-tight tracking-tight">Daily review</h1>
      </div>

      <StatsRow stats={stats.data} />
      <DailyCard />

      <section>
        <h2 className="mb-3 text-[13px] font-semibold uppercase tracking-[0.08em] text-secondary">History</h2>
        <HistoryList
          sessions={sessions.data}
          isLoading={sessions.isLoading}
          isError={sessions.isError}
          onRetry={() => void sessions.refetch()}
        />
      </section>
    </main>
  )
}

function StatsRow({ stats }: { stats: ReviewStats | undefined }) {
  const accuracy = stats?.accuracyPercentage
  return (
    <div className="mb-5 grid grid-cols-2 gap-2.5 sm:grid-cols-4">
      <StatTile label="Current streak" value={stats ? `${stats.currentStreakDays}d` : '—'} />
      <StatTile label="Best streak" value={stats ? `${stats.bestStreakDays}d` : '—'} />
      <StatTile label="Accuracy" value={accuracy == null ? '—' : `${Math.round(accuracy)}%`} />
      <StatTile label="Reviewed" value={stats ? String(stats.totalQuestionsReviewed) : '—'} />
    </div>
  )
}

function StatTile({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-default bg-surface px-4 py-3.5">
      <p className="font-mono text-[10px] uppercase tracking-[0.06em] text-secondary">{label}</p>
      <p className="mt-1 text-[22px] font-bold leading-none tracking-[-0.02em] text-primary">{value}</p>
    </div>
  )
}

// Today's action card. Mirrors the Dashboard banner states, but the call to action enters the runner
// at /review/run. While the queue loads we render nothing — the stats and history stand on their own.
function DailyCard() {
  const { data: questions, isLoading, isError } = useDailyReview()
  const { data: stats } = useReviewStats()

  if (isLoading || isError || !questions) {
    return null
  }

  const count = questions.length

  if (stats?.reviewedToday) {
    return <ReviewedTodayCard streak={stats.currentStreakDays} remaining={count} />
  }

  if (count === 0) {
    return (
      <div className="mb-6 flex items-center gap-2.5 rounded-xl border border-default bg-surface px-[18px] py-3.5 text-[13px] text-secondary">
        <CheckCircle />
        You&apos;re all caught up on reviews today.
      </div>
    )
  }

  return (
    <div
      className="mb-6 flex flex-col gap-3 rounded-xl border px-[18px] py-4 sm:flex-row sm:items-center sm:justify-between"
      style={{
        borderColor: 'rgba(139,92,246,0.25)',
        background: 'linear-gradient(135deg, rgba(139,92,246,0.1), rgba(139,92,246,0.02))',
      }}
    >
      <div className="flex items-center gap-3">
        <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-accent-bg">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--accent-text)" strokeWidth="2" aria-hidden="true">
            <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
            <polyline points="3 3 3 8 8 8" />
          </svg>
        </span>
        <div>
          <p className="text-[15px] font-semibold text-primary">
            {count} {count === 1 ? 'question' : 'questions'} to review today
          </p>
          <p className="mt-0.5 text-[12px] text-secondary">
            Revisit questions you&apos;ve seen before to reinforce what you&apos;ve learned.
          </p>
        </div>
      </div>
      <Link
        to="/review/run"
        className="flex shrink-0 items-center justify-center gap-1.5 rounded-lg bg-accent px-4 py-2 text-[13px] font-medium text-white hover:opacity-90"
      >
        Start review
        <ArrowRight />
      </Link>
    </div>
  )
}

function ReviewedTodayCard({ streak, remaining }: { streak: number; remaining: number }) {
  return (
    <div
      className="mb-6 flex flex-col gap-3 rounded-xl border px-[18px] py-4 sm:flex-row sm:items-center sm:justify-between"
      style={{ borderColor: 'rgba(16,185,129,0.25)', background: 'rgba(16,185,129,0.06)' }}
    >
      <div className="flex items-center gap-3">
        <span
          className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl"
          style={{ backgroundColor: 'rgba(16,185,129,0.12)' }}
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="2.5" aria-hidden="true">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </span>
        <div>
          <p className="text-[15px] font-semibold text-primary">Reviewed today</p>
          <p className="mt-0.5 text-[12px] text-secondary">
            {streak > 0
              ? `You're on a ${streak}-day review streak — keep it going tomorrow.`
              : 'Nice work clearing your review for today.'}
          </p>
        </div>
      </div>
      {remaining > 0 ? (
        <Link
          to="/review/run"
          className="flex shrink-0 items-center justify-center gap-1.5 rounded-lg border border-default px-4 py-2 text-[13px] font-medium text-secondary hover:bg-elevated hover:text-primary"
        >
          Review {remaining} more
        </Link>
      ) : null}
    </div>
  )
}

function HistoryList({
  sessions,
  isLoading,
  isError,
  onRetry,
}: {
  sessions: ReviewSessionSummary[] | undefined
  isLoading: boolean
  isError: boolean
  onRetry: () => void
}) {
  if (isLoading) {
    return <p className="rounded-xl border border-default bg-surface px-[18px] py-4 text-[13px] text-secondary">Loading history…</p>
  }

  if (isError || !sessions) {
    return (
      <div className="flex items-center gap-3 rounded-xl border border-default bg-surface px-[18px] py-4 text-[13px] text-secondary">
        <span className="text-danger">Could not load your review history.</span>
        <button
          type="button"
          onClick={onRetry}
          className="rounded-md border border-strong px-2.5 py-1 text-[12px] font-medium transition-colors hover:bg-elevated"
        >
          Retry
        </button>
      </div>
    )
  }

  if (sessions.length === 0) {
    return (
      <p className="rounded-xl border border-default bg-surface px-[18px] py-4 text-[13px] text-secondary">
        No past sessions yet. Finish a daily review and it&apos;ll show up here.
      </p>
    )
  }

  return (
    <ul className="flex flex-col gap-1.5">
      {sessions.map((session) => (
        <li key={session.id}>
          <HistoryRow session={session} />
        </li>
      ))}
    </ul>
  )
}

function HistoryRow({ session }: { session: ReviewSessionSummary }) {
  const score = session.questionCount > 0 ? Math.round((session.correctCount / session.questionCount) * 100) : 0
  return (
    <Link
      to={`/review/sessions/${session.id}`}
      className="flex items-center gap-4 rounded-lg border border-default bg-surface px-4 py-3 transition-colors hover:border-strong hover:bg-elevated"
    >
      <div className="flex-1">
        <p className="text-[13px] font-medium text-primary">{formatDate(session.completedAt)}</p>
        <p className="mt-0.5 font-mono text-[11px] text-secondary">
          {session.correctCount} / {session.questionCount} correct
        </p>
      </div>
      <span
        className="rounded-full px-2.5 py-1 font-mono text-[12px] font-medium text-accent-text"
        style={{ backgroundColor: 'rgba(139,92,246,0.14)' }}
      >
        {score}%
      </span>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="shrink-0 text-muted">
        <polyline points="9 18 15 12 9 6" />
      </svg>
    </Link>
  )
}

function CheckCircle() {
  return (
    <span className="flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full" style={{ backgroundColor: 'rgba(16,185,129,0.1)' }}>
      <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="3" aria-hidden="true">
        <polyline points="20 6 9 17 4 12" />
      </svg>
    </span>
  )
}

function ArrowRight() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" aria-hidden="true">
      <line x1="5" y1="12" x2="19" y2="12" />
      <polyline points="12 5 19 12 12 19" />
    </svg>
  )
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
