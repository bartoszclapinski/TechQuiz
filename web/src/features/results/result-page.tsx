import { useState } from 'react'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { useStartQuiz } from '../quiz/use-start-quiz'
import { useQuizResult } from './use-quiz-result'
import type { QuizResult, ResultQuestion } from './types'

export function ResultPage() {
  const { attemptId } = useParams<{ attemptId: string }>()
  const { data, isLoading, isError, refetch } = useQuizResult(attemptId ?? '')

  if (!attemptId) {
    return <Navigate to="/categories" replace />
  }

  if (isLoading) {
    return (
      <main className="mx-auto max-w-[720px] px-4 py-8 sm:px-6">
        <p className="text-[15px] text-secondary">Loading result…</p>
      </main>
    )
  }
  if (isError || !data) {
    return (
      <main className="mx-auto max-w-[720px] px-4 py-8 sm:px-6">
        <div className="text-[15px] text-secondary">
          <p className="mb-2 text-danger">Could not load this result.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-pill border border-strong px-4 py-1.5 text-[15px] font-medium transition-colors hover:bg-elevated"
          >
            Retry
          </button>
        </div>
      </main>
    )
  }

  return (
    <main>
      <Result result={data} />
    </main>
  )
}

function Result({ result }: { result: QuizResult }) {
  const navigate = useNavigate()
  const start = useStartQuiz()

  const score = Math.round(result.percentage)
  const best = Math.round(result.bestPercentage)
  const wrongCount = result.totalCount - result.correctCount
  const elapsedSeconds = Math.max(
    0,
    Math.round((new Date(result.completedAt).getTime() - new Date(result.startedAt).getTime()) / 1000),
  )
  const delta =
    result.previousPercentage === null ? null : Math.round(result.percentage - result.previousPercentage)

  return (
    <div className="relative overflow-hidden">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute left-1/2 top-[-160px] h-[520px] w-[820px] -translate-x-1/2 rounded-full"
        style={{ background: 'radial-gradient(circle, var(--hero-glow-2), transparent 65%)' }}
      />
      <div
        aria-hidden="true"
        className="pointer-events-none absolute right-[8%] top-[-100px] h-[380px] w-[380px] rounded-full"
        style={{ background: 'radial-gradient(circle, var(--hero-glow-1), transparent 62%)' }}
      />

      <div className="relative mx-auto flex max-w-[680px] flex-col items-center px-4 pt-10 text-center sm:px-6">
        <span className="mb-6 inline-flex items-center gap-2 rounded-pill bg-amber-bg px-4 py-2 font-mono text-[13px] font-semibold text-amber-text">
          {bandLabel(score)}
        </span>
        <CircularScore score={score} correct={result.correctCount} total={result.totalCount} />
        <h1 className="mt-7 font-display text-[clamp(30px,3.4vw,44px)] font-extrabold leading-[1.1] tracking-[-0.02em]">
          Quiz complete! 🎉
        </h1>
        <p className="mt-2 text-[17px] text-secondary">
          {delta === null ? (
            <>
              First run on <b className="text-primary">{result.categoryName}</b>.
            </>
          ) : delta >= 0 ? (
            <>
              Great run on <b className="text-primary">{result.categoryName}</b> — up {delta}% from last
              time.
            </>
          ) : (
            <>
              Nice work on <b className="text-primary">{result.categoryName}</b>.
            </>
          )}
        </p>

        <div className="mt-7 grid w-full grid-cols-3 gap-3">
          <RewardTile value={`${result.correctCount}/${result.totalCount}`} label="Correct" tone="amber" />
          <RewardTile value={formatDuration(elapsedSeconds)} label="Time" mono />
          <RewardTile
            value={delta === null ? `${best}%` : `${delta >= 0 ? '+' : ''}${delta}%`}
            label={delta === null ? 'Best score' : 'vs last'}
            tone={delta !== null && delta >= 0 ? 'green' : 'default'}
          />
        </div>
      </div>

      <div className="relative mx-auto mt-9 max-w-[720px] px-4 sm:px-6">
        <ReviewSection
          questions={result.questions}
          correctCount={result.correctCount}
          wrongCount={wrongCount}
        />

        <div className="flex flex-wrap justify-center gap-3 border-t border-default pt-6">
          <button
            type="button"
            onClick={() => start.mutate({ id: result.categoryId, name: result.categoryName })}
            disabled={start.isPending}
            aria-busy={start.isPending}
            className="flex items-center gap-2 rounded-pill border border-strong px-6 py-3 text-[15px] font-semibold transition-colors hover:bg-elevated disabled:cursor-not-allowed disabled:opacity-60"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="1 4 1 10 7 10" />
              <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
            </svg>
            {start.isPending ? 'Starting…' : 'Try again'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/dashboard')}
            className="flex items-center gap-2 rounded-pill bg-btn px-7 py-3 text-[15px] font-semibold text-white shadow-float transition-opacity hover:opacity-90"
          >
            Back to dashboard
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
              <line x1="5" y1="12" x2="19" y2="12" />
              <polyline points="12 5 19 12 12 19" />
            </svg>
          </button>
        </div>
      </div>
    </div>
  )
}

// Big circular score badge — the celebration centrepiece (surface disc floating over the glows).
function CircularScore({ score, correct, total }: { score: number; correct: number; total: number }) {
  return (
    <div
      className="flex flex-col items-center justify-center rounded-full border border-default bg-surface shadow-float"
      style={{ width: 'clamp(150px,20vw,190px)', height: 'clamp(150px,20vw,190px)' }}
    >
      <span
        className="font-display font-extrabold leading-none tracking-[-0.03em] text-primary"
        style={{ fontSize: 'clamp(46px,6vw,64px)' }}
      >
        {score}%
      </span>
      <span className="mt-1.5 font-mono text-[13px] text-muted">
        {correct} / {total} correct
      </span>
    </div>
  )
}

function RewardTile({
  value,
  label,
  tone = 'default',
  mono = false,
}: {
  value: string
  label: string
  tone?: 'default' | 'amber' | 'green'
  mono?: boolean
}) {
  const color = tone === 'amber' ? 'text-amber-text' : tone === 'green' ? 'text-success' : 'text-primary'
  return (
    <div className="rounded-[16px] border border-default bg-surface p-4">
      <div className={`text-[22px] font-extrabold ${mono ? 'font-mono' : 'font-display'} ${color}`}>
        {value}
      </div>
      <div className="mt-1 font-mono text-[12px] uppercase tracking-[0.08em] text-muted">{label}</div>
    </div>
  )
}

function ReviewSection({
  questions,
  correctCount,
  wrongCount,
}: {
  questions: ResultQuestion[]
  correctCount: number
  wrongCount: number
}) {
  const [showCorrect, setShowCorrect] = useState(false)

  return (
    <section className="mb-7">
      <div className="mb-3.5 flex items-center justify-between">
        <h2 className="text-[15px] font-semibold tracking-[-0.01em]">Review questions</h2>
        <div className="flex gap-1.5">
          <CountPill className="text-success" bg="rgba(34,197,94,0.12)">
            {correctCount} correct
          </CountPill>
          {wrongCount > 0 && (
            <CountPill className="text-danger" bg="rgba(239,68,68,0.1)">
              {wrongCount} wrong
            </CountPill>
          )}
        </div>
      </div>

      <div className="flex flex-col gap-1.5">
        {questions.map((question, index) =>
          question.isCorrect && !showCorrect ? null : (
            <ReviewRow key={question.questionId} number={index + 1} question={question} />
          ),
        )}

        {correctCount > 0 && (
          <button
            type="button"
            onClick={() => setShowCorrect((value) => !value)}
            className="mt-1.5 rounded-lg border border-dashed border-default p-2.5 font-mono text-[13px] text-secondary transition-colors hover:bg-elevated"
          >
            {showCorrect ? 'Hide correct answers' : `Show ${correctCount} more correct answers`}
          </button>
        )}
      </div>
    </section>
  )
}

function ReviewRow({ number, question }: { number: number; question: ResultQuestion }) {
  const [expanded, setExpanded] = useState(!question.isCorrect)
  const correct = question.isCorrect

  const userOption = question.options.find((option) => option.id === question.userSelectedOptionId)
  const correctOption = question.options.find((option) => option.isCorrect)

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
        style={correct ? undefined : { borderBottom: expanded ? '1px solid rgba(239,68,68,0.35)' : undefined }}
      >
        <StatusIcon correct={correct} />
        <span className="min-w-[30px] font-mono text-[13px] text-secondary">Q{number}</span>
        <p
          className={`flex-1 text-[14px] ${correct ? 'text-secondary' : 'font-medium text-primary'}`}
        >
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
              <span className="font-mono text-[13px] text-muted">No answer</span>
            )}
          </AnswerLine>
          {!correct && correctOption && (
            <AnswerLine label="Correct">
              <AnswerPill text={correctOption.text} tone="success" />
            </AnswerLine>
          )}
          {question.explanation && (
            <div
              className="mt-1 rounded-r border-l-2 px-3 py-2.5"
              style={{ borderColor: 'var(--accent)', background: 'var(--accent-bg)' }}
            >
              <p className="mb-1 font-mono text-[13px] uppercase tracking-[0.06em] text-secondary">Explanation</p>
              <p className="text-[14px] leading-relaxed text-secondary">{question.explanation}</p>
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
      <span className="min-w-[86px] font-mono text-[12px] uppercase tracking-[0.06em] text-secondary">{label}</span>
      {children}
    </div>
  )
}

function AnswerPill({ text, tone }: { text: string; tone: 'success' | 'danger' }) {
  const color = tone === 'success' ? 'text-success' : 'text-danger'
  const bg = tone === 'success' ? 'rgba(34,197,94,0.1)' : 'rgba(239,68,68,0.08)'
  return (
    <code className={`rounded px-2 py-[3px] font-mono text-[13px] ${color}`} style={{ backgroundColor: bg }}>
      {text}
    </code>
  )
}

function CountPill({
  children,
  className,
  bg,
}: {
  children: React.ReactNode
  className: string
  bg: string
}) {
  return (
    <span
      className={`rounded-full px-2 py-[3px] font-mono text-[12px] font-medium ${className}`}
      style={{ backgroundColor: bg }}
    >
      {children}
    </span>
  )
}

function StatusIcon({ correct }: { correct: boolean }) {
  return (
    <span
      className="flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full"
      style={{ backgroundColor: correct ? 'rgba(34,197,94,0.12)' : 'rgba(239,68,68,0.1)' }}
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

function formatDuration(totalSeconds: number): string {
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}

function bandLabel(score: number): string {
  if (score >= 80) return 'Great work'
  if (score >= 60) return 'Good effort'
  if (score >= 40) return 'Keep practicing'
  return 'Keep going'
}
