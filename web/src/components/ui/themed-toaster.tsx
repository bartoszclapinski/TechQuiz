import { Toaster } from 'sonner'
import { useTheme } from '../../theme/use-theme'

// Sonner needs to be told the active theme so toasts match dark/light. Reading it from our
// ThemeProvider keeps the toaster in sync with the same data-theme attribute everything else uses.
export function ThemedToaster() {
  const { theme } = useTheme()
  return <Toaster theme={theme} position="top-right" richColors closeButton />
}
