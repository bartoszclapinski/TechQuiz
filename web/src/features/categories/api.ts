import { apiClient } from '../../lib/api-client'

// Mirrors the API's CategoryDto. userBestScore is a 0..100 percentage; questionCount === 0
// marks a subcategory that has no seeded questions yet ("Coming soon").
export type Category = {
  id: string
  name: string
  description: string
  iconCode: string
  position: number
  questionCount: number
  userBestScore: number
}

// Mirrors the API's TrackDto — a top-level grouping with its ordered subcategories.
export type Track = {
  id: string
  name: string
  description: string
  iconCode: string
  position: number
  categories: Category[]
}

export async function fetchTracks(): Promise<Track[]> {
  const { data } = await apiClient.get<Track[]>('/api/categories')
  return data
}
