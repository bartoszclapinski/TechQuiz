// The dashboard is a single cached read keyed for the current user's session. The time-range
// filter (2.3) will fold its range into this key so each range caches independently.
export const dashboardKey = () => ['dashboard'] as const
