import { useCallback, useEffect, useRef, useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { quizSessionKey } from './query-keys'
import { Difficulty, type DifficultyValue, type QuizRunnerSession } from './types'
import { useSubmitAnswer } from './use-submit-answer'
import { useCompleteQuiz } from './use-complete-quiz'
import { ExitQuizDialog } from './exit-quiz-dialog'

// Difficulty badge styling per ADR-015: emerald/amber/red at ~10% opacity. The text color uses a
// theme-aware token; the tint background is a literal rgba because the color tokens carry no alpha
// channel, so Tailwind opacity modifiers (bg-warning/10) are silently dropped on them.
const DIFFICULTY_META = {
  [Difficulty.Easy]: { label: 'Easy', text: 'text-success', bg: 'rgba(34,197,94,0.14)' },
  [Difficulty.Medium]: { label: 'Medium', text: 'text-amber-text', bg: 'var(--amber-bg)' },
  [Difficulty.Hard]: { label: 'Hard', text: 'text-danger', bg: 'rgba(239,68,68,0.14)' },
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

  const { mutateAsync: submitAnswerAsync } = useSubmitAnswer()
  const { mutateAsync: completeAsync, isPending: isCompleting } = useCompleteQuiz()

  // Holds the in-flight save for the latest selection so completion can await it — a fast submit on
  // the last question must not let /complete outrun the last /answer and score a missing answer.
  const pendingSaveRef = useRef<Promise<unknown> | null>(null)
  // Synchronous latch: a rapid double-Enter (or Enter racing the native button click) could observe
  // isCompleting === false twice before React re-renders the disabled button and fire /complete twice.
  const completingRef = useRef(false)

  const total = session.questions.length
  const isLast = currentIndex === total - 1

  const selectAnswer = useCallback(
    (questionId: string, optionId: string) => {
      setAnswers((prev) => ({ ...prev, [questionId]: optionId }))
      const save = submitAnswerAsync({ attemptId, questionId, selectedOptionId: optionId })
      pendingSaveRef.current = save
      // Roll the optimistic selection back if the save fails — but only if the user hasn't since
      // picked a different option for this question, otherwise we'd clobber the newer choice.
      save.catch(() => {
        setAnswers((prev) => {
          if (prev[questionId] !== optionId) return prev
          const next = { ...prev }
          delete next[questionId]
          return next
        })
        toast.error('Could not save your answer. Please pick it again.')
      })
    },
    [attemptId, submitAnswerAsync],
  )

  const handleAdvance = useCallback(async () => {
    if (!isLast) {
      setCurrentIndex((index) => index + 1)
      return
    }
    if (completingRef.current) return
    completingRef.current = true
    try {
      // Wait for the last answer to persist before scoring; if it rejected, the catch above already
      // rolled it back and toasted, so abort rather than complete with a missing answer.
      await pendingSaveRef.current
      await completeAsync(attemptId)
    } catch {
      completingRef.current = false
    }
  }, [attemptId, completeAsync, isLast])

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
  // Fallback keeps a difficulty outside 0-2 (a bad/extended enum from the API) from white-screening
  // the runner on the undefined lookup.
  const difficulty = DIFFICULTY_META[question.difficulty] ?? DIFFICULTY_META[Difficulty.Medium]
  const selectedOptionId = answers[question.id]
  const progress = ((currentIndex + 1) / total) * 100

  return (
    <div className="flex min-h-screen flex-col bg-base text-primary">
      <header className="border-b border-default bg-surface">
        <div className="mx-auto flex max-w-[900px] items-center gap-4 px-5 py-3.5 sm:px-8">
          <span className="whitespace-nowrap font-mono text-[13px] font-semibold text-secondary">
            {session.categoryName} · {currentIndex + 1} / {total}
          </span>
          <div className="h-[7px] flex-1 overflow-hidden rounded-pill bg-track">
            <div
              className="h-full rounded-pill bg-brand transition-[width] duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
          {/* Potential XP on offer for this quiz — 10 per correct answer (ADR-025). */}
          <span
            title="XP you can earn — 10 per correct answer"
            className="hidden whitespace-nowrap rounded-pill bg-amber-bg px-3 py-1.5 font-mono text-[12px] font-semibold text-amber-text sm:inline"
          >
            +{total * 10} XP
          </span>
          <button
            type="button"
            onClick={() => setExitOpen(true)}
            aria-label="Exit quiz"
            className="flex h-[34px] w-[34px] shrink-0 items-center justify-center rounded-[10px] border border-strong text-secondary transition-colors hover:bg-elevated hover:text-primary"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>
      </header>

      <div className="flex flex-1 flex-col items-center justify-center px-5 py-12 sm:px-8">
        <div className="w-full max-w-[660px]">
          <div className="mb-7">
            <span
              className={`mb-5 inline-block rounded-pill px-2.5 py-[5px] font-mono text-[11px] font-semibold uppercase tracking-[0.08em] ${difficulty.text}`}
              style={{ backgroundColor: difficulty.bg }}
            >
              {difficulty.label}
            </span>
            <h2 className="font-display text-[clamp(24px,2.6vw,34px)] font-extrabold leading-[1.25] tracking-[-0.01em]">
              {question.text}
            </h2>
          </div>

          <div className="flex flex-col gap-2.5">
            {question.options.map((option, index) => {
              const selected = selectedOptionId === option.id
              return (
                <button
                  key={option.id}
                  type="button"
                  onClick={() => selectAnswer(question.id, option.id)}
                  aria-pressed={selected}
                  className={`flex items-center gap-4 rounded-[16px] border bg-surface px-5 py-4 text-left text-[16px] transition-all ${
                    selected
                      ? 'border-accent shadow-focus'
                      : 'border-default hover:border-strong'
                  }`}
                >
                  <span
                    className={`flex h-[30px] w-[30px] shrink-0 items-center justify-center rounded-[9px] font-mono text-[14px] font-bold ${
                      selected ? 'bg-btn text-white' : 'bg-elevated text-muted'
                    }`}
                  >
                    {index + 1}
                  </span>
                  <span>{option.text}</span>
                </button>
              )
            })}
          </div>

          <div className="mt-7 flex items-center justify-end gap-4 border-t border-default pt-6 sm:justify-between">
            {/* Keyboard tip is desktop-only guidance — phones have no 1-4/Enter shortcuts. */}
            <p className="hidden font-mono text-[13px] text-muted sm:block">
              Press <Kbd>1-4</Kbd> to select, <Kbd>Enter</Kbd> to continue
            </p>
            <button
              type="button"
              onClick={handleAdvance}
              disabled={!selectedOptionId || isCompleting}
              className="flex items-center gap-1.5 rounded-pill bg-btn px-6 py-3 text-[15px] font-semibold text-white shadow-float transition-opacity hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-40"
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
    <kbd className="rounded bg-elevated px-1.5 py-0.5 font-mono text-[12px] text-secondary">{children}</kbd>
  )
}
