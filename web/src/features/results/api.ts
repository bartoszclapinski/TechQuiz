import { apiClient } from '../../lib/api-client'
import type { QuizResult } from './types'

export async function fetchQuizResult(attemptId: string): Promise<QuizResult> {
  const { data } = await apiClient.get<QuizResult>(`/api/quizzes/${attemptId}/result`)
  return data
}
