import { apiClient } from '../../lib/api-client'
import type { QuizSession } from './types'

export async function startQuiz(categoryId: string): Promise<QuizSession> {
  const { data } = await apiClient.post<QuizSession>('/api/quizzes/start', { categoryId })
  return data
}

export async function submitAnswer(
  attemptId: string,
  questionId: string,
  selectedOptionId: string | null,
): Promise<void> {
  await apiClient.post(`/api/quizzes/${attemptId}/answer`, { questionId, selectedOptionId })
}

export async function completeQuiz(attemptId: string): Promise<void> {
  await apiClient.post(`/api/quizzes/${attemptId}/complete`)
}
