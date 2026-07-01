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

// Mirrors the API's ReviewStatsDto — review-specific aggregates (kept apart from quiz stats since the
// daily queue is small). accuracyPercentage is null until at least one question has been reviewed.
export type ReviewStats = {
  totalSessions: number
  totalQuestionsReviewed: number
  accuracyPercentage: number | null
  currentStreakDays: number
  bestStreakDays: number
  reviewedToday: boolean
}

// Mirrors the API's ReviewSessionSummary — one row in the history list. Correctness is a count over the
// session's items (derived server-side); no per-item detail here.
export type ReviewSessionSummary = {
  id: string
  completedAt: string
  questionCount: number
  correctCount: number
}

// Mirrors the API's ReviewSessionItemDto — one graded question inside a past session. Same shape the
// post-grade summary renders from, so both views share one row component. Correctness is derived on the
// server (never persisted); options still carry no IsCorrect.
export type ReviewSessionItem = {
  questionId: string
  questionText: string
  category: string
  difficulty: DifficultyValue
  options: ReviewOption[]
  selectedOptionId: string | null
  correctOptionId: string
  isCorrect: boolean
  explanation: string
}

// Mirrors the API's ReviewSessionDetailDto — a past session's full graded results, re-read on demand.
export type ReviewSessionDetail = {
  id: string
  completedAt: string
  items: ReviewSessionItem[]
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

export async function fetchReviewStats(): Promise<ReviewStats> {
  const { data } = await apiClient.get<ReviewStats>('/api/review/stats')
  return data
}

export async function fetchReviewSessions(): Promise<ReviewSessionSummary[]> {
  const { data } = await apiClient.get<ReviewSessionSummary[]>('/api/review/sessions')
  return data
}

export async function fetchReviewSession(id: string): Promise<ReviewSessionDetail> {
  const { data } = await apiClient.get<ReviewSessionDetail>(`/api/review/sessions/${id}`)
  return data
}
