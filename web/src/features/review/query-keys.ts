// The daily-review queue is a single per-user read; the Dashboard card and the runner share this
// one key so opening the review from the card reuses the cached queue (no second fetch).
export const dailyReviewKey = ['review', 'daily'] as const

// Review-specific stats (totals, accuracy, streaks, reviewed-today). Grading invalidates both this
// and the queue key so the Dashboard reflects the session immediately.
export const reviewStatsKey = ['review', 'stats'] as const

// The user's past review sessions (history list on the hub). Grading invalidates this so a
// just-completed session shows up immediately.
export const reviewSessionsKey = ['review', 'sessions'] as const

// One past session's graded detail, keyed by id. Sessions are immutable once completed, so this
// never needs invalidation.
export const reviewSessionKey = (id: string) => ['review', 'session', id] as const
