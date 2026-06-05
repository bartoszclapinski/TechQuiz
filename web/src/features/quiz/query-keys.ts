// A started quiz session is cached under its attempt id so the QuizPage can read the questions
// that useStartQuiz already fetched, rather than refetching on navigation.
export const quizSessionKey = (attemptId: string) => ['quiz-session', attemptId] as const
