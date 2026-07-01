import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import type { AuthTokens } from '../features/auth/types'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8085'

// withCredentials lets the browser send and receive the HttpOnly refresh cookie.
export const apiClient = axios.create({
  baseURL,
  withCredentials: true,
})

// The access token lives only in this module's memory — never localStorage. An XSS
// payload can read storage but not a closure variable, so this keeps the token off-limits.
let accessToken: string | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

export function getAccessToken() {
  return accessToken
}

// AuthContext registers this so a failed refresh can tear down React auth state.
let onRefreshFailure: (() => void) | null = null

export function setOnRefreshFailure(handler: (() => void) | null) {
  onRefreshFailure = handler
}

apiClient.interceptors.request.use((config) => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

// Hits the cookie-based refresh endpoint and stores the new access token. A single
// in-flight promise is shared so a burst of concurrent 401s triggers one refresh, not N.
let refreshPromise: Promise<string> | null = null

export async function refreshAccessToken(): Promise<string> {
  refreshPromise ??= apiClient
    .post<AuthTokens>('/api/auth/refresh')
    .then((response) => {
      setAccessToken(response.data.accessToken)
      return response.data.accessToken
    })
    .finally(() => {
      refreshPromise = null
    })
  return refreshPromise
}

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean }

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetriableConfig | undefined
    const isRefreshCall = original?.url?.includes('/api/auth/refresh')

    // Retry once on 401 — but never for the refresh call itself: a 401 there means the
    // refresh cookie is gone or expired, i.e. a real logout rather than an access-token blip.
    if (error.response?.status === 401 && original && !original._retry && !isRefreshCall) {
      original._retry = true
      try {
        const token = await refreshAccessToken()
        original.headers.Authorization = `Bearer ${token}`
        return apiClient(original)
      } catch {
        setAccessToken(null)
        onRefreshFailure?.()
      }
    }

    return Promise.reject(error)
  },
)
