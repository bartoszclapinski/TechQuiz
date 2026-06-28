// Centralizes the code-challenges query key, mirroring the pool/categories key factories.
// Run and grade are mutations with no cached entity to invalidate, so only the list has a key.
export const codeChallengesKey = () => ['code-challenges'] as const
