// A completed attempt's result is immutable, so it's cached under its attempt id and never refetched.
export const quizResultKey = (attemptId: string) => ['quiz-result', attemptId] as const
