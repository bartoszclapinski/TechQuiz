import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Difficulty, type DifficultyValue, type ReviewGradeResult, type ReviewQuestion } from './api'
import { useDailyReview } from './use-daily-review'
import { useGradeReview } from './use-grade-review'
import { ReviewResultView, type ReviewResultItem } from './review-summary'

// Difficulty badge styling per ADR-015, mirroring the quiz runner.
const DIFFICULTY_META = {
  [Difficulty.Easy]: { label: 'Easy', text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  [Difficulty.Medium]: { label: 'Medium', text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  [Difficulty.Hard]: { label: 'Hard', text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
} satisfies Record<DifficultyValue, { label: string; text: string; bg: string }>

// The focused, topbar-less runner at /review/run. Entered from the hub; on finish it returns there.
export function ReviewRunnerPage() {
  const { data: questions, isLoading, isError, refetch } = useDailyReview()

  if (isLoading) {
    return <CenteredMessage>Loading your review…</CenteredMessage>
  }

  if (isError || !questions) {
    return (
      <CenteredMessage>
        <p className="mb-2 text-danger">Could not load your daily review.</p>
        <button
          type="button"
          onClick={() => void refetch()}
          className="rounded-md border border-strong px-3 py-1.5 text-sm font-medium transition-colors hover:bg-elevated"
        >
          Retry
        </button>
      </CenteredMessage>
    )
  }

  if (questions.length === 0) {
    return <CaughtUp />
  }

  return <ReviewSession questions={questions} />
}

function ReviewSession({ questions: initialQuestions }: { questions: ReviewQuestion[] }) {
  // Freeze the queue for the lifetime of the session: grading invalidates the daily-review query,
  // and without this snapshot a background refetch (shorter queue) would reshape the runner and the
  // summary mid-session. A review plays a fixed set, so capturing it once is the correct model.
  const [questions] = useState(initialQuestions)
  const [results, setResults] = useState<ReviewGradeResult[] | null>(null)
  const [answers, setAnswers] = useState<Record<string, string>>({})

  if (results) {
    return <RunnerSummary questions={questions} answers={answers} results={results} />
  }

  return <ReviewRunner questions={questions} answers={answers} setAnswers={setAnswers} onGraded={setResults} />
}

function ReviewRunner({
  questions,
  answers,
  setAnswers,
  onGraded,
}: {
  questions: ReviewQuestion[]
  answers: Record<string, string>
  setAnswers: React.Dispatch<React.SetStateAction<Record<string, string>>>
  onGraded: (results: ReviewGradeResult[]) => void
}) {
  const navigate = useNavigate()
  const [currentIndex, setCurrentIndex] = useState(0)
  const { mutateAsync: gradeAsync, isPending } = useGradeReview()

  // Synchronous latch so a rapid double-Enter can't fire two grade calls before the re-render.
  const submittingRef = useRef(false)

  const total = questions.length
  const isLast = currentIndex === total - 1

  const selectAnswer = useCallback(
    (questionId: string, optionId: string) => {
      // The review is stateless — answers live only in component memory until we grade on submit.
      setAnswers((prev) => ({ ...prev, [questionId]: optionId }))
    },
    [setAnswers],
  )

  const handleAdvance = useCallback(async () => {
    if (!isLast) {
      setCurrentIndex((index) => index + 1)
      return
    }
    if (submittingRef.current) return
    submittingRef.current = true
    try {
      const payload = questions.map((question) => ({
        questionId: question.id,
        selectedOptionId: answers[question.id] ?? null,
      }))
      const results = await gradeAsync(payload)
      onGraded(results)
    } catch {
      submittingRef.current = false
    }
  }, [answers, gradeAsync, isLast, onGraded, questions])

  // Keyboard control mirrors the quiz runner: 1-4 selects, Enter advances, Esc exits.
  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      const question = questions[currentIndex]
      if (event.key >= '1' && event.key <= '4') {
        const option = question.options[Number(event.key) - 1]
        if (option) selectAnswer(question.id, option.id)
      } else if (event.key === 'Enter') {
        if (answers[question.id]) void handleAdvance()
      } else if (event.key === 'Escape') {
        navigate('/review')
      }
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [questions, currentIndex, answers, selectAnswer, handleAdvance, navigate])

  const question = questions[currentIndex]
  const difficulty = DIFFICULTY_META[question.difficulty] ?? DIFFICULTY_META[Difficulty.Medium]
  const selectedOptionId = answers[question.id]
  const progress = ((currentIndex + 1) / total) * 100

  return (
    <div className="flex min-h-screen flex-col bg-base text-primary">
      <header className="flex items-center justify-between gap-4 border-b border-default px-6 py-3">
        <div className="flex flex-1 items-center gap-3">
          <span className="whitespace-nowrap font-mono text-[11px] font-medium text-secondary">
            Daily review · {question.category} · {currentIndex + 1} of {total}
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
          onClick={() => navigate('/review')}
          aria-label="Exit review"
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
              disabled={!selectedOptionId || isPending}
              className="flex items-center gap-1.5 rounded-lg bg-accent px-[18px] py-2.5 text-[13px] font-medium text-white transition-opacity disabled:cursor-not-allowed disabled:opacity-40"
            >
              {isLast ? (isPending ? 'Grading…' : 'Finish review') : 'Next'}
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
    </div>
  )
}

function RunnerSummary({
  questions,
  answers,
  results,
}: {
  questions: ReviewQuestion[]
  answers: Record<string, string>
  results: ReviewGradeResult[]
}) {
  const navigate = useNavigate()
  const items = useMemo<ReviewResultItem[]>(() => {
    const resultByQuestion = new Map(results.map((result) => [result.questionId, result]))
    return questions.flatMap((question) => {
      const result = resultByQuestion.get(question.id)
      if (!result) return []
      return [
        {
          questionId: question.id,
          questionText: question.text,
          difficulty: question.difficulty,
          options: question.options,
          selectedOptionId: answers[question.id] ?? null,
          correctOptionId: result.correctOptionId,
          isCorrect: result.isCorrect,
          explanation: result.explanation,
        },
      ]
    })
  }, [questions, answers, results])

  return (
    <div className="min-h-screen bg-base text-primary">
      <ReviewResultView
        eyebrow="Review complete"
        title="Daily review"
        items={items}
        footer={
          <button
            type="button"
            onClick={() => navigate('/review')}
            className="flex items-center gap-2 rounded-lg bg-accent px-5 py-2.5 text-[14px] font-medium text-white transition-opacity hover:opacity-90"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="19" y1="12" x2="5" y2="12" />
              <polyline points="12 19 5 12 12 5" />
            </svg>
            Back to review
          </button>
        }
      />
    </div>
  )
}

function CaughtUp() {
  const navigate = useNavigate()
  return (
    <CenteredMessage>
      <p className="mb-1 text-[15px] font-semibold text-primary">You&apos;re all caught up</p>
      <p className="mb-4 text-sm text-secondary">Nothing to review today — great work.</p>
      <button
        type="button"
        onClick={() => navigate('/review')}
        className="rounded-lg bg-accent px-5 py-2.5 text-[14px] font-medium text-white transition-opacity hover:opacity-90"
      >
        Back to review
      </button>
    </CenteredMessage>
  )
}

function CenteredMessage({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-base px-6 text-center text-primary">
      <div>{children}</div>
    </div>
  )
}

function Kbd({ children }: { children: React.ReactNode }) {
  return (
    <kbd className="rounded bg-elevated px-1.5 py-0.5 font-mono text-[10px] text-secondary">{children}</kbd>
  )
}
