import { Link, useNavigate, useParams } from 'react-router-dom'
import { useReviewSession } from './use-review-session'
import { ReviewResultView } from './review-summary'

// A past session's graded results, re-read from the API and rendered with the shared summary layout.
// Reached from the hub's history list; keeps the topbar (this is a browsable page, not the runner).
export function ReviewSessionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: session, isLoading, isError, refetch } = useReviewSession(id ?? '')

  if (isLoading) {
    return <CenteredMessage>Loading session…</CenteredMessage>
  }

  if (isError || !session) {
    return (
      <CenteredMessage>
        <p className="mb-3 text-danger">Could not load this review session.</p>
        <div className="flex items-center justify-center gap-2.5">
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-md border border-strong px-3 py-1.5 text-[15px] font-medium transition-colors hover:bg-elevated"
          >
            Retry
          </button>
          <Link to="/review" className="rounded-md border border-default px-3 py-1.5 text-[15px] font-medium text-secondary hover:bg-elevated hover:text-primary">
            Back to review
          </Link>
        </div>
      </CenteredMessage>
    )
  }

  return (
    <ReviewResultView
      eyebrow={formatDate(session.completedAt)}
      title="Review session"
      items={session.items}
      footer={<BackButton />}
    />
  )
}

function BackButton() {
  const navigate = useNavigate()
  return (
    <button
      type="button"
      onClick={() => navigate('/review')}
      className="flex items-center gap-2 rounded-lg bg-accent px-5 py-2.5 text-[15px] font-medium text-white transition-opacity hover:opacity-90"
    >
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <line x1="19" y1="12" x2="5" y2="12" />
        <polyline points="12 19 5 12 12 5" />
      </svg>
      Back to review
    </button>
  )
}

function CenteredMessage({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center px-6 text-center text-primary">
      <div>{children}</div>
    </div>
  )
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
