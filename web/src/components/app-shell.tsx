import { useEffect, useState } from 'react'
import { NavLink, Outlet, useMatch } from 'react-router-dom'
import { useAuth } from '../features/auth/use-auth'
import { ThemeToggle } from './ui/theme-toggle'

function initialsFromEmail(email: string): string {
  const local = email.split('@')[0] ?? ''
  const parts = local.split(/[._-]+/).filter(Boolean)
  const letters = parts.length >= 2 ? parts[0][0] + parts[1][0] : local.slice(0, 2)
  return letters.toUpperCase()
}

// Single source of truth for the primary navigation, rendered two ways: the horizontal bar at md+ and
// the mobile drawer below md. Keeping one list means the two never drift apart.
const NAV_ITEMS: { to: string; label: string }[] = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/categories', label: 'Categories' },
  { to: '/generate', label: 'Generate' },
  { to: '/pool', label: 'Pool' },
  { to: '/challenges', label: 'Challenges' },
  { to: '/history', label: 'History' },
  { to: '/review', label: 'Daily review' },
]

// Route-aware shell. The quiz runner and the daily-review runner are focused, distraction-free screens
// (ADR-014), so on /quiz/:id and /review/run we render only the Outlet and skip the topbar entirely.
// The review hub (/review) and a past session (/review/sessions/:id) keep the topbar — they're browsable.
export function AppShell() {
  const { user, logout } = useAuth()
  const onQuizRoute = useMatch('/quiz/:id') !== null
  const onReviewRunRoute = useMatch('/review/run') !== null

  const [menuOpen, setMenuOpen] = useState(false)

  // Lock body scroll while the drawer is open so the page behind doesn't scroll under the overlay.
  useEffect(() => {
    document.body.style.overflow = menuOpen ? 'hidden' : ''
    return () => {
      document.body.style.overflow = ''
    }
  }, [menuOpen])

  if (onQuizRoute || onReviewRunRoute) {
    return <Outlet />
  }

  return (
    <div className="min-h-screen bg-base text-primary">
      <header className="flex items-center gap-4 border-b border-default bg-surface px-4 py-3.5 sm:gap-7 sm:px-6">
        <NavLink to="/categories" className="flex items-center gap-2">
          <span className="flex h-[26px] w-[26px] items-center justify-center rounded-md bg-accent text-sm font-bold text-white">
            T
          </span>
          <span className="text-[15px] font-semibold tracking-tight">TechQuiz</span>
        </NavLink>

        {/* Desktop nav — hidden on mobile, where the hamburger drawer takes over. */}
        <nav className="hidden flex-1 items-center gap-1 md:flex">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `rounded-md px-2.5 py-1.5 text-[13px] font-medium ${
                  isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex flex-1 items-center justify-end gap-2.5 md:flex-none">
          {/* Settings + avatar live in the desktop bar; on mobile they move into the drawer. */}
          <NavLink
            to="/settings"
            aria-label="Settings"
            title="Settings"
            className={({ isActive }) =>
              `hidden h-7 w-7 items-center justify-center rounded-md transition-colors md:flex ${
                isActive ? 'bg-accent-bg text-accent-text' : 'text-secondary hover:text-primary'
              }`
            }
          >
            <SettingsIcon />
          </NavLink>
          <ThemeToggle />
          <button
            type="button"
            onClick={() => void logout()}
            aria-label="Log out"
            title="Log out"
            className="hidden h-7 w-7 items-center justify-center rounded-full bg-accent-bg text-[11px] font-semibold text-accent-text md:flex"
          >
            {user ? initialsFromEmail(user.email) : '?'}
          </button>

          {/* Hamburger — mobile only. */}
          <button
            type="button"
            onClick={() => setMenuOpen(true)}
            aria-label="Open menu"
            aria-expanded={menuOpen}
            className="flex h-9 w-9 items-center justify-center rounded-md text-secondary hover:text-primary md:hidden"
          >
            <MenuIcon />
          </button>
        </div>
      </header>

      {menuOpen ? (
        <MobileMenu
          initials={user ? initialsFromEmail(user.email) : '?'}
          email={user?.email}
          onClose={() => setMenuOpen(false)}
          onLogout={() => void logout()}
        />
      ) : null}

      <Outlet />
    </div>
  )
}

// Slide-in drawer for phones: a backdrop that dismisses on tap and a right-anchored panel with the
// nav links plus settings + logout. Each link calls onClose on tap so navigating dismisses the drawer.
function MobileMenu({
  initials,
  email,
  onClose,
  onLogout,
}: {
  initials: string
  email: string | undefined
  onClose: () => void
  onLogout: () => void
}) {
  return (
    <div className="fixed inset-0 z-50 md:hidden">
      <button
        type="button"
        aria-label="Close menu"
        onClick={onClose}
        className="absolute inset-0 bg-black/50"
      />
      <div className="absolute right-0 top-0 flex h-full w-72 max-w-[82%] flex-col border-l border-default bg-surface">
        <div className="flex items-center justify-between border-b border-default px-4 py-3.5">
          <div className="flex items-center gap-2.5">
            <span className="flex h-8 w-8 items-center justify-center rounded-full bg-accent-bg text-[12px] font-semibold text-accent-text">
              {initials}
            </span>
            {email ? (
              <span className="max-w-[150px] truncate text-[13px] text-secondary">{email}</span>
            ) : null}
          </div>
          <button
            type="button"
            aria-label="Close menu"
            onClick={onClose}
            className="flex h-9 w-9 items-center justify-center rounded-md text-secondary hover:text-primary"
          >
            <CloseIcon />
          </button>
        </div>

        <nav className="flex flex-1 flex-col gap-0.5 overflow-y-auto p-2">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              onClick={onClose}
              className={({ isActive }) =>
                `flex min-h-[44px] items-center rounded-md px-3 text-[15px] font-medium ${
                  isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
          <NavLink
            to="/settings"
            onClick={onClose}
            className={({ isActive }) =>
              `flex min-h-[44px] items-center gap-2 rounded-md px-3 text-[15px] font-medium ${
                isActive ? 'bg-accent-bg text-primary' : 'text-secondary hover:text-primary'
              }`
            }
          >
            <SettingsIcon />
            Settings
          </NavLink>
        </nav>

        <div className="border-t border-default p-2">
          <button
            type="button"
            onClick={onLogout}
            className="flex min-h-[44px] w-full items-center rounded-md px-3 text-[15px] font-medium text-secondary hover:text-primary"
          >
            Log out
          </button>
        </div>
      </div>
    </div>
  )
}

function SettingsIcon() {
  return (
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
  )
}

function MenuIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
      <line x1="3" y1="6" x2="21" y2="6" />
      <line x1="3" y1="12" x2="21" y2="12" />
      <line x1="3" y1="18" x2="21" y2="18" />
    </svg>
  )
}

function CloseIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
      <line x1="18" y1="6" x2="6" y2="18" />
      <line x1="6" y1="6" x2="18" y2="18" />
    </svg>
  )
}
