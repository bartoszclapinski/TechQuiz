// Centralizes the pool query key so the browse read and the publish invalidation stay in
// sync, mirroring the categories/ai-keys key factories.
export const pooledQuestionsKey = () => ['pooled-questions'] as const
