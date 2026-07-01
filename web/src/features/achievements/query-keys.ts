// The badge catalogue is a single cached read for the current user's session. Completing a quiz or
// grading a review invalidates it so a freshly-earned badge appears without a manual refresh.
export const achievementsKey = ['achievements'] as const
