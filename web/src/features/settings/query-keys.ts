// Centralizes the AI-keys query key so the list read and the set/remove invalidations stay
// consistent, mirroring the categories feature's categoriesKey factory.
export const aiKeysKey = () => ['ai-keys'] as const
