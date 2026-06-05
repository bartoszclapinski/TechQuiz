import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import {
  apiClient,
  refreshAccessToken,
  setAccessToken,
  setOnRefreshFailure,
} from '../../lib/api-client'
import { AuthContext, type AuthContextValue, type AuthStatus } from './auth-context'
import { decodeUserFromToken } from './jwt'
import type { AuthTokens, User } from './types'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [status, setStatus] = useState<AuthStatus>('loading')

  const applyAccessToken = useCallback((token: string) => {
    setAccessToken(token)
    const decoded = decodeUserFromToken(token)
    setUser(decoded)
    setStatus(decoded === null ? 'unauthenticated' : 'authenticated')
  }, [])

  const clearSession = useCallback(() => {
    setAccessToken(null)
    setUser(null)
    setStatus('unauthenticated')
  }, [])

  const login = useCallback(
    async (email: string, password: string) => {
      const { data } = await apiClient.post<AuthTokens>('/api/auth/login', { email, password })
      applyAccessToken(data.accessToken)
    },
    [applyAccessToken],
  )

  const register = useCallback(
    async (email: string, password: string) => {
      const { data } = await apiClient.post<AuthTokens>('/api/auth/register', { email, password })
      applyAccessToken(data.accessToken)
    },
    [applyAccessToken],
  )

  const logout = useCallback(async () => {
    try {
      await apiClient.post('/api/auth/logout')
    } finally {
      clearSession()
    }
  }, [clearSession])

  // The 401 interceptor calls this when a background refresh fails — keep React in sync.
  useEffect(() => {
    setOnRefreshFailure(clearSession)
    return () => setOnRefreshFailure(null)
  }, [clearSession])

  // Bootstrap: a valid refresh cookie means a returning user, so silently restore the
  // session on first load. The ref guard keeps StrictMode's double-invoke from refreshing twice.
  const bootstrapped = useRef(false)
  useEffect(() => {
    if (bootstrapped.current) {
      return
    }
    bootstrapped.current = true
    refreshAccessToken()
      .then(applyAccessToken)
      .catch(() => setStatus('unauthenticated'))
  }, [applyAccessToken])

  const value = useMemo<AuthContextValue>(
    () => ({ user, status, login, register, logout }),
    [user, status, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
