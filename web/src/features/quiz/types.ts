// In-quiz projections. Options omit IsCorrect and questions omit Explanation — the API never
// leaks correct answers while a quiz is active (CLAUDE.md hard rule #4). Enums arrive as numbers
// (the API has no string-enum converter), so Difficulty/QuestionType are numeric here.
export const Difficulty = { Easy: 0, Medium: 1, Hard: 2 } as const
export type DifficultyValue = (typeof Difficulty)[keyof typeof Difficulty]

export type QuizOption = {
  id: string
  text: string
  orderIndex: number
}

export type QuizQuestion = {
  id: string
  type: number
  difficulty: number
  text: string
  options: QuizOption[]
}

export type QuizSession = {
  attemptId: string
  questions: QuizQuestion[]
}
