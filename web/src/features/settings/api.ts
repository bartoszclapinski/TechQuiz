import { apiClient } from '../../lib/api-client'

// Provider ids must match the API's AiProviderKind enum names exactly — they cross the wire as
// strings (e.g. "Anthropic", "OpenAi"), since the AI endpoints deliberately have no global
// string-enum converter (the quiz client depends on numeric Difficulty).
export type AiProvider = {
  id: string
  label: string
  // Only providers with a live backend client can actually hold a usable key. The rest render as
  // "soon" — there is no point storing a key for a provider we cannot call yet (matches the
  // topbar's COMING_SOON affordance, ADR-014).
  available: boolean
}

export const AI_PROVIDERS: readonly AiProvider[] = [
  { id: 'Anthropic', label: 'Anthropic (Claude)', available: true },
  { id: 'OpenAi', label: 'OpenAI (GPT)', available: false },
  { id: 'Gemini', label: 'Google (Gemini)', available: false },
  { id: 'OpenRouter', label: 'OpenRouter', available: false },
]

// Returns the provider kinds the current user has a key configured for — names only, never the
// key material (the API never serializes it back, per ADR-006).
export async function fetchConfiguredProviders(): Promise<string[]> {
  const { data } = await apiClient.get<string[]>('/api/ai/keys')
  return data
}

export async function setAiKey(provider: string, apiKey: string): Promise<void> {
  await apiClient.put('/api/ai/keys', { provider, apiKey })
}

export async function removeAiKey(provider: string): Promise<void> {
  await apiClient.delete(`/api/ai/keys/${provider}`)
}
