import { useCallback, useEffect, useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { quizSessionKey } from './query-keys'
import { Difficulty, type DifficultyValue, type QuizRunnerSession } from './types'
import { useSubmitAnswer } from './use-submit-answer'
import { useCompleteQuiz } from './use-complete-quiz'
import { ExitQuizDialog } from './exit-quiz-dialog'

// Difficulty badge styling per ADR-015: emerald/amber/red at ~10% opacity. The text color uses a
// theme-aware token; the tint background is a literal rgba because the color tokens carry no alpha
// channel, so Tailwind opacity modifiers (bg-warning/10) are silently dropped on them.
const DIFFICULTY_META = {
  [Difficulty.Easy]: { label: 'Easy', text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  [Difficulty.Medium]: { label: 'Medium', text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  [Difficulty.Hard]: { label: 'Hard', text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
} satisfies Record<DifficultyValue, { label: string; text: string; bg: string }>

export function QuizPage() {
  const { id } = useParams<{ id: string }>()
  const queryClient = useQueryClient()

  // The session was seeded into the cache by useStartQuiz before navigating here. There is no
  // queryFn behind this key and the cache is memory-only, so a hard refresh or deep-link has no
  // data to render — redirect to Categories (known MVP limitation; see iteration 1.6 notes).
  const session = queryClient.getQueryData<QuizRunnerSession>(quizSessionKey(id ?? ''))
  if (!id || !session) {
    return <Navigate to="/categories" replace />
  }

  return <QuizRunner attemptId={id} session={session} />
}

function QuizRunner({ attemptId, session }: { attemptId: string; session: QuizRunnerSession }) {
  const navigate = useNavigate()
  const [currentIndex, setCurrentIndex] = useState(0)
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [exitOpen, setExitOpen] = useState(false)

  const { mutate: submitAnswerMutate } = useSubmitAnswer()
  const completeQuiz = useCompleteQuiz()
  const { mutate: completeMutate, isPending: isCompleting } = completeQuiz

  const total = session.questions.length
  const isLast = currentIndex === total - 1

  const selectAnswer = useCallback(
    (questionId: string, optionId: string) => {
      setAnswers((prev) => ({ ...prev, [questionId]: optionId }))
      submitAnswerMutate({ attemptId, questionId, selectedOptionId: optionId })
    },
    [attemptId, submitAnswerMutate],
  )

  const handleAdvance = useCallback(() => {
    if (isCompleting) return
    if (currentIndex === total - 1) {
      completeMutate(attemptId)
    } else {
      setCurrentIndex((index) => index + 1)
    }
  }, [attemptId, completeMutate, currentIndex, isCompleting, total])

  // Global keyboard control (ADR-015): 1-4 selects, Enter advances, Esc opens the exit modal.
  // Suspended while the modal is open so its own Esc/handlers take over.
  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (exitOpen) return
      const question = session.questions[currentIndex]
      if (event.key >= '1' && event.key <= '4') {
        const option = question.options[Number(event.key) - 1]
        if (option) selectAnswer(question.id, option.id)
      } else if (event.key === 'Enter') {
        if (answers[question.id]) handleAdvance()
      } else if (event.key === 'Escape') {
        setExitOpen(true)
      }
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [session, currentIndex, answers, exitOpen, selectAnswer, handleAdvance])

  const question = session.questions[currentIndex]
  const difficulty = DIFFICULTY_META[question.difficulty]
  const selectedOptionId = answers[question.id]
  const progress = ((currentIndex + 1) / total) * 100

  return (
    <div className="flex min-h-screen flex-col bg-base text-primary">
      <header className="flex items-center justify-between gap-4 border-b border-default px-6 py-3">
        <div className="flex flex-1 items-center gap-3">
          <span className="whitespace-nowrap font-mono text-[11px] font-medium text-secondary">
            {session.categoryName} · {currentIndex + 1} of {total}
          </span>
          <div className="h-[3px] flex-1 overflow-hidden rounded-full bg-elevated">
            <div
              className="h-full rounded-full bg-accent transition-[width] duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
        <button
          type="button"
          onClick={() => setExitOpen(true)}
          aria-label="Exit quiz"
          className="flex h-7 w-7 items-center justify-center rounded-md border border-default text-secondary transition-colors hover:bg-elevated"
        >
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </header>

      <div className="flex flex-1 flex-col items-center justify-center px-6 py-12">
        <div className="w-full max-w-[600px]">
          <div className="mb-6">
            <span
              className={`mb-3.5 inline-block rounded-full px-2 py-[3px] font-mono text-[10px] font-medium uppercase tracking-[0.04em] ${difficulty.text}`}
              style={{ backgroundColor: difficulty.bg }}
            >
              {difficulty.label}
            </span>
            <h2 className="text-[22px] font-semibold leading-[1.3]">{question.text}</h2>
          </div>

          <div className="flex flex-col gap-2">
            {question.options.map((option, index) => {
              const selected = selectedOptionId === option.id
              return (
                <button
                  key={option.id}
                  type="button"
                  onClick={() => selectAnswer(question.id, option.id)}
                  aria-pressed={selected}
                  className={`flex items-center gap-3.5 rounded-[10px] border bg-surface px-[18px] py-3.5 text-left text-[14px] transition-colors ${
                    selected
                      ? 'border-accent shadow-[0_0_0_3px_rgba(139,92,246,0.15)]'
                      : 'border-default hover:border-strong'
                  }`}
                >
                  <span
                    className={`flex h-6 w-6 shrink-0 items-center justify-center rounded font-mono text-[12px] font-semibold ${
                      selected ? 'bg-accent text-white' : 'bg-base text-muted'
                    }`}
                  >
                    {index + 1}
                  </span>
                  <span>{option.text}</span>
                </button>
              )
            })}
          </div>

          <div className="mt-8 flex items-center justify-between gap-4 border-t border-default pt-5">
            <p className="font-mono text-[11px] text-secondary">
              Tip: press <Kbd>1-4</Kbd> to select, <Kbd>Enter</Kbd> to continue
            </p>
            <button
              type="button"
              onClick={handleAdvance}
              disabled={!selectedOptionId || isCompleting}
              className="flex items-center gap-1.5 rounded-lg bg-accent px-[18px] py-2.5 text-[13px] font-medium text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
            >
              {isLast ? 'Submit quiz' : 'Next'}
              {!isLast && (
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <line x1="5" y1="12" x2="19" y2="12" />
                  <polyline points="12 5 19 12 12 19" />
                </svg>
              )}
            </button>
          </div>
        </div>
      </div>

      <ExitQuizDialog
        open={exitOpen}
        onOpenChange={setExitOpen}
        onConfirm={() => navigate('/categories')}
      />
    </div>
  )
}

function Kbd({ children }: { children: React.ReactNode }) {
  return (
    <kbd className="rounded bg-elevated px-1.5 py-0.5 font-mono text-[10px] text-secondary">{children}</kbd>
  )
}
