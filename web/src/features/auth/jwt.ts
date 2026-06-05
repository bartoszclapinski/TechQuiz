import type { User } from './types'

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const segments = token.split('.')
  if (segments.length !== 3) {
    return null
  }
  try {
    const base64 = segments[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
    return JSON.parse(atob(padded)) as Record<string, unknown>
  } catch {
    return null
  }
}

// The access token carries the user's id (sub) and email as registered JWT claims, so the
// SPA derives the current user from the token itself — no extra /me round-trip needed.
export function decodeUserFromToken(token: string): User | null {
  const payload = decodeJwtPayload(token)
  if (payload === null) {
    return null
  }
  const id = typeof payload.sub === 'string' ? payload.sub : null
  const email = typeof payload.email === 'string' ? payload.email : null
  return id !== null && email !== null ? { id, email } : null
}
