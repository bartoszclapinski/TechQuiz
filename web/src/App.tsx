import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { LoginPage } from './features/auth/login-page'
import { RegisterPage } from './features/auth/register-page'
import { RequireAuth } from './features/auth/require-auth'
import { AppShell } from './components/app-shell'
import { CategoriesPage } from './features/categories/categories-page'
import { QuizPage } from './features/quiz/quiz-page'
import { ResultPage } from './features/results/result-page'
import { SettingsPage } from './features/settings/settings-page'
import { GeneratePage } from './features/generate/generate-page'
import { PoolPage } from './features/pool/pool-page'
import { CodeChallengesPage } from './features/code-challenges/code-challenges-page'
import { CodeChallengePage } from './features/code-challenges/code-challenge-page'
import { DashboardPage } from './features/dashboard/dashboard-page'
import { HistoryPage } from './features/history/history-page'
import { ReviewHubPage } from './features/review/review-hub-page'
import { ReviewRunnerPage } from './features/review/review-runner-page'
import { ReviewSessionDetailPage } from './features/review/review-session-detail-page'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
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
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/quiz/:id" element={<QuizPage />} />
            <Route path="/result/:attemptId" element={<ResultPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/categories" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
