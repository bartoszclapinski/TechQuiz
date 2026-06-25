import { apiClient } from '../../lib/api-client'

// Mirrors the API's PooledQuestionResponse. Difficulty is a name ("Easy"/"Medium"/"Hard"),
// matching the generate preview — the pool endpoints map the enum to its name rather than
// leaning on a global string-enum converter (the quiz client needs numeric Difficulty).
// There is no correct-answer field by design (hard rule #4): the pool is a catalogue, not a key.
export type PooledQuestion = {
  id: string
  stem: string
  options: string[]
  difficulty: string
  explanation: string | null
  provider: string
  topic: string
  createdByUserId: string
  generatedAtUtc: string
}

export async function fetchPooledQuestions(): Promise<PooledQuestion[]> {
  const { data } = await apiClient.get<PooledQuestion[]>('/api/pool/questions')
  return data
}

export async function publishPooledQuestion(id: string): Promise<void> {
  await apiClient.post(`/api/pool/questions/${id}/publish`)
}
