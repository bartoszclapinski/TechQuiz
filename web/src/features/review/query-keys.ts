// The daily-review queue is a single per-user read; the Dashboard card and the runner share this
// one key so opening the review from the card reuses the cached queue (no second fetch).
export const dailyReviewKey = ['review', 'daily'] as const
