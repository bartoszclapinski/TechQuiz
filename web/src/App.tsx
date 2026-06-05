import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { LoginPage } from './features/auth/login-page'
import { RegisterPage } from './features/auth/register-page'
import { RequireAuth } from './features/auth/require-auth'
import { CategoriesPage } from './features/categories/categories-page'
import { QuizPage } from './features/quiz/quiz-page'
import { ResultPage } from './features/results/result-page'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<RequireAuth />}>
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/quiz/:id" element={<QuizPage />} />
          <Route path="/result/:attemptId" element={<ResultPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/categories" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
