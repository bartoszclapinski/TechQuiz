import type { User } from './types'

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const segments = token.split('.')
  if (segments.length !== 3) {
    return null
  }
  try {
    const base64 = segments[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
    // atob yields a binary string; decode through TextDecoder so multi-byte UTF-8 claims
    // (e.g. a non-ASCII email) survive instead of being mangled by atob + JSON.parse.
    const bytes = Uint8Array.from(atob(padded), (char) => char.charCodeAt(0))
    return JSON.parse(new TextDecoder().decode(bytes)) as Record<string, unknown>
  } catch {
    return null
  }
}

// The access token carries the user's id (sub) and email as registered JWT claims, so the
// SPA derives the current user from the token itself — no extra /me round-trip needed.
// This is display-only: the payload is base64-decoded WITHOUT verifying the signature, so
// never treat the returned id as an authz input on the client. The server validates the
// token (signature + expiry) on every request — that is the only trust boundary.
export function decodeUserFromToken(token: string): User | null {
  const payload = decodeJwtPayload(token)
  if (payload === null) {
    return null
  }
  const id = typeof payload.sub === 'string' ? payload.sub : null
  const email = typeof payload.email === 'string' ? payload.email : null
  return id !== null && email !== null ? { id, email } : null
}
