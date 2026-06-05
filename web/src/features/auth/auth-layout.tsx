import type { ReactNode } from 'react'
import { ThemeToggle } from '../../components/ui/theme-toggle'
import { AuthHero } from './auth-hero'

// Split-screen frame shared by Login and Register: a form column on the left (logo + theme
// toggle header, the page's form as children, a footer) and the decorative AuthHero on the
// right. The hero collapses below lg so the form takes the full width on narrow screens.
export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="grid min-h-screen bg-base lg:grid-cols-2">
      <div className="relative flex flex-col p-8 sm:p-12">
        <header className="mb-12 flex items-center justify-between lg:mb-16">
          <div className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-accent text-lg font-bold text-white">
              T
            </div>
            <span className="text-lg font-semibold tracking-tight">TechQuiz</span>
          </div>
          <ThemeToggle />
        </header>

        <div className="flex flex-1 flex-col justify-center">
          <div className="w-full max-w-[360px]">{children}</div>
        </div>

        <footer className="pt-6">
          <p className="font-mono text-[11px] text-muted">© 2026 TechQuiz · v0.1.0</p>
        </footer>
      </div>

      <AuthHero />
    </div>
  )
}
