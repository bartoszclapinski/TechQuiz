import type { ReactNode } from 'react'
import { ThemeToggle } from '../../components/ui/theme-toggle'
import { AuthHero } from './auth-hero'

// Split-screen frame shared by Login and Register: a form column on the left (logo + theme-toggle
// header, the page's form as children, a footer) and the decorative AuthHero on the right. Both
// content blocks are nudged toward the middle gutter (the form right-aligned in its column, the hero
// left-aligned in its) so the two halves read together rather than hugging the outer edges. Centered
// on mobile, where the hero collapses and the form takes the full width.
export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="grid min-h-screen bg-base lg:grid-cols-2">
      <div className="relative flex flex-col p-8 sm:p-12">
        <header className="mb-12 flex items-center justify-between lg:mb-16">
          <div className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 items-center justify-center rounded-[11px] bg-brand font-display text-[18px] font-extrabold text-brandfg">
              T
            </div>
            <span className="font-display text-[18px] font-bold tracking-tight">TechQuiz</span>
          </div>
          <ThemeToggle />
        </header>

        <div className="flex flex-1 flex-col justify-center">
          <div className="mx-auto w-full max-w-[400px] lg:ml-auto lg:mr-28">{children}</div>
        </div>

        <footer className="pt-6">
          <p className="font-mono text-[13px] text-muted">© 2026 TechQuiz · v0.1.0</p>
        </footer>
      </div>

      <AuthHero />
    </div>
  )
}
