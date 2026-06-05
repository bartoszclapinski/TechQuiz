import { apiClient } from '../../lib/api-client'

// Mirrors the API's CategoryDto. userBestScore is a 0..100 percentage; questionCount === 0
// marks a category that has no seeded questions yet ("Coming soon").
export type Category = {
  id: string
  name: string
  description: string
  iconCode: string
  questionCount: number
  userBestScore: number
}

export async function fetchCategories(): Promise<Category[]> {
  const { data } = await apiClient.get<Category[]>('/api/categories')
  return data
}
