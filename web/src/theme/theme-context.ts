import { createContext } from 'react'

export type Theme = 'dark' | 'light'

export type ThemeContextValue = {
  theme: Theme
  setTheme: (theme: Theme) => void
  toggleTheme: () => void
}

// Split from the provider component so the module exports no components —
// keeps react-refresh (Fast Refresh) happy and avoids a lint warning.
export const ThemeContext = createContext<ThemeContextValue | null>(null)
