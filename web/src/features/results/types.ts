import type { DifficultyValue } from '../quiz/types'

// Result projections DO expose isCorrect on options and the per-question explanation — unlike the
// in-quiz DTOs (quiz/types.ts), which hide both. The quiz is over, so revealing answers is allowed
// (CLAUDE.md hard rule #4 only forbids leaking them while a quiz is active).
export type ResultOption = {
  id: string
  text: string
  orderIndex: number
  isCorrect: boolean
}

export type ResultQuestion = {
  questionId: string
  text: string
  difficulty: DifficultyValue
  explanation: string
  options: ResultOption[]
  userSelectedOptionId: string | null
  isCorrect: boolean
}

export type DifficultyBreakdown = {
  correct: number
  total: number
}

export type QuizResult = {
  attemptId: string
  categoryId: string
  categoryName: string
  startedAt: string
  completedAt: string
  correctCount: number
  totalCount: number
  percentage: number
  bestPercentage: number
  // Null on the first completed attempt for this quiz — there is nothing prior to compare against.
  previousPercentage: number | null
  // XP earned for this attempt (correct answers × 10) — ADR-025.
  xpEarned: number
  // Keys are numeric Difficulty values serialized as strings (the API has no string-enum converter).
  byDifficulty: Record<string, DifficultyBreakdown>
  questions: ResultQuestion[]
}
