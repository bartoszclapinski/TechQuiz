import { createContext } from 'react'
import type { User } from './types'

// loading covers the initial bootstrap (attempting a silent refresh) so the UI can hold a
// spinner instead of flashing the login screen for a returning user.
export type AuthStatus = 'loading' | 'authenticated' | 'unauthenticated'

export type AuthContextValue = {
  user: User | null
  status: AuthStatus
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
