// Mirrors the API's AuthTokensDto. Dates arrive as ISO strings over JSON.
export type AuthTokens = {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export type User = {
  id: string
  email: string
}
