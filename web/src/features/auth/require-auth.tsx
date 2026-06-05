import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './use-auth'
import { FullPageSpinner } from '../../components/ui/full-page-spinner'

// Layout route guarding the authenticated section. While the AuthProvider is still attempting
// its silent boot refresh (status 'loading'), hold a spinner instead of bouncing to /login —
// otherwise a returning user with a valid refresh cookie would flash the login screen before
// the refresh resolves. Once unauthenticated, redirect to /login and remember where we came
// from so the login flow can send the user back.
export function RequireAuth() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'loading') {
    return <FullPageSpinner />
  }

  if (status === 'unauthenticated') {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}
