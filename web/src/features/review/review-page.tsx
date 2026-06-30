import { useCallback, useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Difficulty, type DifficultyValue, type ReviewGradeResult, type ReviewQuestion } from './api'
import { useDailyReview } from './use-daily-review'
import { useGradeReview } from './use-grade-review'

// Difficulty badge styling per ADR-015, mirroring the quiz runner.
const DIFFICULTY_META = {
  [Difficulty.Easy]: { label: 'Easy', text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  [Difficulty.Medium]: { label: 'Medium', text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  [Difficulty.Hard]: { label: 'Hard', text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
} satisfies Record<DifficultyValue, { label: string; text: string; bg: string }>

export function ReviewPage() {
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

function ReviewSession({ questions }: { questions: ReviewQuestion[] }) {
  const [results, setResults] = useState<ReviewGradeResult[] | null>(null)
  const [answers, setAnswers] = useState<Record<string, string>>({})

  if (results) {
    return <ReviewSummary questions={questions} answers={answers} results={results} />
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
        navigate('/dashboard')
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
          onClick={() => navigate('/dashboard')}
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

function ReviewSummary({
  questions,
  answers,
  results,
}: {
  questions: ReviewQuestion[]
  answers: Record<string, string>
  results: ReviewGradeResult[]
}) {
  const navigate = useNavigate()
  const resultByQuestion = new Map(results.map((result) => [result.questionId, result]))
  const correctCount = results.filter((result) => result.isCorrect).length
  const total = results.length
  const score = total > 0 ? Math.round((correctCount / total) * 100) : 0

  return (
    <div className="min-h-screen bg-base text-primary">
      <main className="mx-auto max-w-[800px] px-6 py-8 sm:px-9">
        <div className="mb-2">
        <p className="mb-1.5 font-mono text-[11px] uppercase tracking-[0.1em] text-secondary">Review complete</p>
        <h1 className="text-2xl font-semibold leading-tight tracking-tight">Daily review</h1>
      </div>

      <div
        className="mb-6 mt-5 flex items-center justify-between rounded-[14px] border px-8 py-7"
        style={{
          borderColor: 'rgba(139,92,246,0.2)',
          background: 'linear-gradient(135deg, rgba(139,92,246,0.08), rgba(139,92,246,0.02))',
        }}
      >
        <div>
          <p className="mb-1 font-mono text-[12px] uppercase tracking-[0.08em] text-secondary">You got</p>
          <div className="flex items-baseline gap-1">
            <span className="text-[56px] font-bold leading-none tracking-[-0.04em]">{correctCount}</span>
            <span className="text-2xl font-semibold text-accent-text">/ {total}</span>
          </div>
          <p className="mt-2 text-[12px] font-medium text-muted">{score}% correct</p>
        </div>
        <span
          className="rounded-full px-3 py-1.5 font-mono text-[11px] font-medium tracking-[0.04em] text-accent-text"
          style={{ backgroundColor: 'rgba(139,92,246,0.18)' }}
        >
          {bandLabel(score)}
        </span>
      </div>

      <section className="mb-7 flex flex-col gap-1.5">
        {questions.map((question, index) => {
          const result = resultByQuestion.get(question.id)
          if (!result) return null
          return (
            <SummaryRow
              key={question.id}
              number={index + 1}
              question={question}
              result={result}
              selectedOptionId={answers[question.id] ?? null}
            />
          )
        })}
      </section>

      <div className="flex gap-2.5 border-t border-default pt-5">
        <button
          type="button"
          onClick={() => navigate('/dashboard')}
          className="flex items-center gap-2 rounded-lg bg-accent px-5 py-2.5 text-[14px] font-medium text-white transition-opacity hover:opacity-90"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="19" y1="12" x2="5" y2="12" />
            <polyline points="12 19 5 12 12 5" />
          </svg>
          Back to dashboard
        </button>
        </div>
      </main>
    </div>
  )
}

function SummaryRow({
  number,
  question,
  result,
  selectedOptionId,
}: {
  number: number
  question: ReviewQuestion
  result: ReviewGradeResult
  selectedOptionId: string | null
}) {
  const [expanded, setExpanded] = useState(!result.isCorrect)
  const correct = result.isCorrect
  const userOption = question.options.find((option) => option.id === selectedOptionId)
  const correctOption = question.options.find((option) => option.id === result.correctOptionId)

  return (
    <div
      className="overflow-hidden rounded-lg border bg-surface"
      style={{ borderColor: correct ? undefined : 'rgba(239,68,68,0.35)' }}
    >
      <button
        type="button"
        onClick={() => setExpanded((value) => !value)}
        aria-expanded={expanded}
        className="flex w-full items-center gap-3 px-3.5 py-2.5 text-left"
      >
        <StatusIcon correct={correct} />
        <span className="min-w-[30px] font-mono text-[11px] text-secondary">Q{number}</span>
        <p className={`flex-1 text-[13px] ${correct ? 'text-secondary' : 'font-medium text-primary'}`}>
          {question.text}
        </p>
        <svg
          width="13"
          height="13"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          className={`shrink-0 text-muted transition-transform ${expanded ? 'rotate-90' : ''}`}
        >
          <polyline points="9 18 15 12 9 6" />
        </svg>
      </button>

      {expanded && (
        <div className="flex flex-col gap-2.5 bg-base px-3.5 py-3.5 pl-12">
          <AnswerLine label="Your answer">
            {userOption ? (
              <AnswerPill text={userOption.text} tone={correct ? 'success' : 'danger'} />
            ) : (
              <span className="font-mono text-[12px] text-muted">No answer</span>
            )}
          </AnswerLine>
          {!correct && correctOption && (
            <AnswerLine label="Correct">
              <AnswerPill text={correctOption.text} tone="success" />
            </AnswerLine>
          )}
          {result.explanation && (
            <div
              className="mt-1 rounded-r border-l-2 px-3 py-2.5"
              style={{ borderColor: 'var(--accent)', background: 'rgba(139,92,246,0.06)' }}
            >
              <p className="mb-1 font-mono text-[11px] uppercase tracking-[0.06em] text-secondary">Explanation</p>
              <p className="text-[13px] leading-relaxed text-secondary">{result.explanation}</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

function AnswerLine({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2.5">
      <span className="min-w-[86px] font-mono text-[10px] uppercase tracking-[0.06em] text-secondary">{label}</span>
      {children}
    </div>
  )
}

function AnswerPill({ text, tone }: { text: string; tone: 'success' | 'danger' }) {
  const color = tone === 'success' ? 'text-success' : 'text-danger'
  const bg = tone === 'success' ? 'rgba(16,185,129,0.08)' : 'rgba(239,68,68,0.08)'
  return (
    <code className={`rounded px-2 py-[3px] font-mono text-[12px] ${color}`} style={{ backgroundColor: bg }}>
      {text}
    </code>
  )
}

function StatusIcon({ correct }: { correct: boolean }) {
  return (
    <span
      className="flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full"
      style={{ backgroundColor: correct ? 'rgba(16,185,129,0.1)' : 'rgba(239,68,68,0.1)' }}
    >
      {correct ? (
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="3">
          <polyline points="20 6 9 17 4 12" />
        </svg>
      ) : (
        <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="var(--danger)" strokeWidth="3">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      )}
    </span>
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
        onClick={() => navigate('/dashboard')}
        className="rounded-lg bg-accent px-5 py-2.5 text-[14px] font-medium text-white transition-opacity hover:opacity-90"
      >
        Back to dashboard
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

function bandLabel(score: number): string {
  if (score >= 80) return 'Great work'
  if (score >= 60) return 'Good effort'
  if (score >= 40) return 'Keep practicing'
  return 'Keep going'
}
