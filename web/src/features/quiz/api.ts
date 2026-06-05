import { apiClient } from '../../lib/api-client'
import type { QuizSession } from './types'

export async function startQuiz(categoryId: string): Promise<QuizSession> {
  const { data } = await apiClient.post<QuizSession>('/api/quizzes/start', { categoryId })
  return data
}
