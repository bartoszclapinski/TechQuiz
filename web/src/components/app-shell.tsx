import { NavLink, Outlet, useMatch } from 'react-router-dom'
import { useAuth } from '../features/auth/use-auth'
import { ThemeToggle } from './ui/theme-toggle'

// Phase 2/3 destinations live in the topbar as disabled "soon" items per ADR-014, so the nav
// shape is final from day one and users see what's coming.
const COMING_SOON = ['Daily review', 'Generate', 'Dashboard', 'History']

function initialsFromEmail(email: string): string {
  const local = email.split('@')[0] ?? ''
  const parts = local.split(/[._-]+/).filter(Boolean)
  const letters = parts.length >= 2 ? parts[0][0] + parts[1][0] : local.slice(0, 2)
  return letters.toUpperCase()
}

// Route-aware shell. The quiz runner is a focused, distraction-free screen (ADR-014), so on
// /quiz/:id we render only the Outlet and skip the topbar entirely.
export function AppShell() {
  const { user, logout } = useAuth()
  const onQuizRoute = useMatch('/quiz/:id') !== null

  if (onQuizRoute) {
    return <Outlet />
  }

  return (
    <div className="min-h-screen bg-base text-primary">
      <header className="flex items-center gap-7 border-b border-default bg-surface px-6 py-3.5">
        <NavLink to="/categories" className="flex items-center gap-2">
          <span className="flex h-[26px] w-[26px] items-center justify-center rounded-md bg-accent text-sm font-bold text-white">
            T
          </span>
          <span className="text-[15px] font-semibold tracking-tight">TechQuiz</span>
        </NavLink>

        <nav className="flex flex-1 items-center gap-1">
          <NavLink
            to="/categories"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Categories
          </NavLink>
          {COMING_SOON.map((label) => (
            <span
              key={label}
              aria-disabled="true"
              className="flex cursor-default items-center gap-1.5 rounded-md px-2.5 py-1.5 text-[13px] font-medium text-muted opacity-60"
            >
              {label}
              <span className="rounded-full bg-elevated px-1.5 py-px font-mono text-[9px] text-secondary">
                soon
              </span>
            </span>
          ))}
        </nav>

        <div className="flex items-center gap-2.5">
          <ThemeToggle />
          <button
            type="button"
            onClick={() => void logout()}
            aria-label="Log out"
            title="Log out"
            className="flex h-7 w-7 items-center justify-center rounded-full bg-accent-bg text-[11px] font-semibold text-accent-text"
          >
            {user ? initialsFromEmail(user.email) : '?'}
          </button>
        </div>
      </header>

      <Outlet />
    </div>
  )
}
