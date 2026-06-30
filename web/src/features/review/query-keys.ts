// The daily-review queue is a single per-user read; the Dashboard card and the runner share this
// one key so opening the review from the card reuses the cached queue (no second fetch).
export const dailyReviewKey = ['review', 'daily'] as const

// Review-specific stats (totals, accuracy, streaks, reviewed-today). Grading invalidates both this
// and the queue key so the Dashboard reflects the session immediately.
export const reviewStatsKey = ['review', 'stats'] as const
