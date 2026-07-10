import { lazy } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { LandingPage } from './features/landing/landing-page'
import { LoginPage } from './features/auth/login-page'
import { RequireAuth } from './features/auth/require-auth'
import { AppShell } from './components/app-shell'

// Landing, Login and the layout wrappers stay eager (entry points + always-needed chrome). Every
// authed page is code-split so the initial bundle stays small and heavy screens (the Monaco code
// editor especially) only load when visited (perf, iteration 4.5). AppShell renders the Suspense
// fallback around its Outlet, so a page chunk loads without unmounting the shell.
const DashboardPage = lazy(() =>
  import('./features/dashboard/dashboard-page').then((m) => ({ default: m.DashboardPage })),
)
const HistoryPage = lazy(() =>
  import('./features/history/history-page').then((m) => ({ default: m.HistoryPage })),
)
const ReviewHubPage = lazy(() =>
  import('./features/review/review-hub-page').then((m) => ({ default: m.ReviewHubPage })),
)
const ReviewRunnerPage = lazy(() =>
  import('./features/review/review-runner-page').then((m) => ({ default: m.ReviewRunnerPage })),
)
const ReviewSessionDetailPage = lazy(() =>
  import('./features/review/review-session-detail-page').then((m) => ({
    default: m.ReviewSessionDetailPage,
  })),
)
const CategoriesPage = lazy(() =>
  import('./features/categories/categories-page').then((m) => ({ default: m.CategoriesPage })),
)
const GeneratePage = lazy(() =>
  import('./features/generate/generate-page').then((m) => ({ default: m.GeneratePage })),
)
const PoolPage = lazy(() =>
  import('./features/pool/pool-page').then((m) => ({ default: m.PoolPage })),
)
const CodeChallengesPage = lazy(() =>
  import('./features/code-challenges/code-challenges-page').then((m) => ({
    default: m.CodeChallengesPage,
  })),
)
const CodeChallengePage = lazy(() =>
  import('./features/code-challenges/code-challenge-page').then((m) => ({
    default: m.CodeChallengePage,
  })),
)
const DashboardSettings = lazy(() =>
  import('./features/settings/settings-page').then((m) => ({ default: m.SettingsPage })),
)
const QuizPage = lazy(() =>
  import('./features/quiz/quiz-page').then((m) => ({ default: m.QuizPage })),
)
const ResultPage = lazy(() =>
  import('./features/results/result-page').then((m) => ({ default: m.ResultPage })),
)

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        {/* Public registration is closed during the demo (iteration 4.11) — the API rejects it too.
            Keep the path as a redirect so any stale link/bookmark lands on sign-in. */}
        <Route path="/register" element={<Navigate to="/login" replace />} />
        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/history" element={<HistoryPage />} />
            <Route path="/review" element={<ReviewHubPage />} />
            <Route path="/review/run" element={<ReviewRunnerPage />} />
            <Route path="/review/sessions/:id" element={<ReviewSessionDetailPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/generate" element={<GeneratePage />} />
            <Route path="/pool" element={<PoolPage />} />
            <Route path="/challenges" element={<CodeChallengesPage />} />
            <Route path="/challenges/:id" element={<CodeChallengePage />} />
            <Route path="/settings" element={<DashboardSettings />} />
            <Route path="/quiz/:id" element={<QuizPage />} />
            <Route path="/result/:attemptId" element={<ResultPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
