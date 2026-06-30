import { apiClient } from '../../lib/api-client'

// Enums arrive as numbers (the API has no string-enum converter), matching the quiz runner.
export const Difficulty = { Easy: 0, Medium: 1, Hard: 2 } as const
export type DifficultyValue = (typeof Difficulty)[keyof typeof Difficulty]

export type ReviewOption = {
  id: string
  text: string
  orderIndex: number
}

// Mirrors the API's ReviewQuestionDto — the in-quiz shape with a category (the queue is mixed-
// category) and, deliberately, no correctness.
export type ReviewQuestion = {
  id: string
  type: number
  difficulty: DifficultyValue
  text: string
  category: string
  options: ReviewOption[]
}

// One answer the user gives during the session; selectedOptionId is null only if skipped.
export type ReviewAnswer = {
  questionId: string
  selectedOptionId: string | null
}

// Mirrors the API's ReviewGradeResultDto — correctness is revealed only here, after submit.
export type ReviewGradeResult = {
  questionId: string
  selectedOptionId: string | null
  correctOptionId: string
  isCorrect: boolean
  explanation: string
}

export const DAILY_REVIEW_COUNT = 10

export async function fetchDailyReview(count: number = DAILY_REVIEW_COUNT): Promise<ReviewQuestion[]> {
  const { data } = await apiClient.get<ReviewQuestion[]>('/api/review/daily', { params: { count } })
  return data
}

export async function gradeReview(answers: ReviewAnswer[]): Promise<ReviewGradeResult[]> {
  const { data } = await apiClient.post<ReviewGradeResult[]>('/api/review/grade', { answers })
  return data
}
