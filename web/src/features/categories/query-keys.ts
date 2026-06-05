// Centralizes the categories query key so cache reads/writes and invalidations stay consistent,
// mirroring the quiz feature's quizSessionKey factory.
export const categoriesKey = () => ['categories'] as const
