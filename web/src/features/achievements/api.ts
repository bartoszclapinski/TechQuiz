import { apiClient } from '../../lib/api-client'

// Mirrors the API's AchievementDto / AchievementsDto. Every badge is derived on read: `progress` is
// already clamped to `target` server-side, and `unlocked` is `progress` having reached `target`.
// `group` clusters badges and picks an icon (`quiz` / `review` / `streak`) without the UI hardcoding
// individual keys.
export type AchievementGroup = 'quiz' | 'review' | 'streak'

export type Achievement = {
  key: string
  title: string
  description: string
  group: AchievementGroup
  target: number
  progress: number
  unlocked: boolean
}

export type Achievements = {
  unlockedCount: number
  totalCount: number
  items: Achievement[]
}

export async function fetchAchievements(): Promise<Achievements> {
  const { data } = await apiClient.get<Achievements>('/api/achievements')
  return data
}
