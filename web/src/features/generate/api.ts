import { apiClient } from '../../lib/api-client'

// Difficulty and provider cross the wire as enum *names* ("Easy", "Anthropic") — the AI endpoints
// have no global string-enum converter (the quiz client depends on numeric Difficulty), so this
// feature works in names, deliberately separate from the numeric quiz feature.
export type GenerateRequest = {
  topic: string
  difficulty: string
  count: number
  provider: string
}

// Mirrors the API's GeneratedDraftDto. No correct-answer key — the draft contract omits
// CorrectOptionIndex by design (hard rule #4), so the preview can't (and shouldn't) mark an option.
export type GeneratedDraft = {
  stem: string
  options: string[]
  difficulty: string
  explanation: string | null
}

export type GenerateResult = {
  provider: string
  questions: GeneratedDraft[]
}

export async function generateQuestions(request: GenerateRequest): Promise<GenerateResult> {
  const { data } = await apiClient.post<GenerateResult>('/api/ai/questions', request)
  return data
}
