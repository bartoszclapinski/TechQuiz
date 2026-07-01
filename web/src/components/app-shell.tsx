import { NavLink, Outlet, useMatch } from 'react-router-dom'
import { useAuth } from '../features/auth/use-auth'
import { ThemeToggle } from './ui/theme-toggle'

function initialsFromEmail(email: string): string {
  const local = email.split('@')[0] ?? ''
  const parts = local.split(/[._-]+/).filter(Boolean)
  const letters = parts.length >= 2 ? parts[0][0] + parts[1][0] : local.slice(0, 2)
  return letters.toUpperCase()
}

// Route-aware shell. The quiz runner and the daily-review runner are focused, distraction-free screens
// (ADR-014), so on /quiz/:id and /review/run we render only the Outlet and skip the topbar entirely.
// The review hub (/review) and a past session (/review/sessions/:id) keep the topbar — they're browsable.
export function AppShell() {
  const { user, logout } = useAuth()
  const onQuizRoute = useMatch('/quiz/:id') !== null
  const onReviewRunRoute = useMatch('/review/run') !== null

  if (onQuizRoute || onReviewRunRoute) {
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
            to="/dashboard"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Dashboard
          </NavLink>
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
          <NavLink
            to="/generate"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Generate
          </NavLink>
          <NavLink
            to="/pool"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Pool
          </NavLink>
          <NavLink
            to="/challenges"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Challenges
          </NavLink>
          <NavLink
            to="/history"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            History
          </NavLink>
          <NavLink
            to="/review"
            className={({ isActive }) =>
              `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            Daily review
          </NavLink>
        </nav>

        <div className="flex items-center gap-2.5">
          <NavLink
            to="/settings"
            aria-label="Settings"
            title="Settings"
            className={({ isActive }) =>
              `flex h-7 w-7 items-center justify-center rounded-md transition-colors ${
                isActive ? 'bg-accent-bg text-accent-text' : 'text-secondary hover:text-primary'
              }`
            }
          >
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <circle cx="12" cy="12" r="3" />
              <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
            </svg>
          </NavLink>
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
