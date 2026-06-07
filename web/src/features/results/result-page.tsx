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

  return (
    <main className="mx-auto max-w-[800px] px-6 py-8 sm:px-9">
      {isLoading ? (
        <p className="text-sm text-secondary">Loading result…</p>
      ) : isError || !data ? (
        <div className="text-sm text-secondary">
          <p className="mb-2 text-danger">Could not load this result.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="rounded-md border border-strong px-3 py-1.5 text-sm font-medium transition-colors hover:bg-elevated"
          >
            Retry
          </button>
        </div>
      ) : (
        <Result result={data} />
      )}
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
  const avgSeconds = result.totalCount > 0 ? Math.round(elapsedSeconds / result.totalCount) : 0

  return (
    <>
      <div className="mb-2">
        <p className="mb-1.5 font-mono text-[11px] uppercase tracking-[0.1em] text-secondary">Quiz complete</p>
        <h1 className="text-2xl font-semibold leading-tight tracking-tight">{result.categoryName}</h1>
      </div>

      <ScoreHero score={score} previousPercentage={result.previousPercentage} percentage={result.percentage} />

      <div className="mb-7 grid grid-cols-2 gap-2 sm:grid-cols-4">
        <MetricCard label="Correct">
          {result.correctCount} <span className="text-[13px] font-normal text-muted">/ {result.totalCount}</span>
        </MetricCard>
        <MetricCard label="Time" mono>
          {formatDuration(elapsedSeconds)}
        </MetricCard>
        <MetricCard label="Avg / question" mono>
          {avgSeconds}
          <span className="text-[12px] text-muted">s</span>
        </MetricCard>
        <MetricCard label="Best score">
          {best}
          <span className="text-[12px] text-muted">%</span>
        </MetricCard>
      </div>

      <ReviewSection
        questions={result.questions}
        correctCount={result.correctCount}
        wrongCount={wrongCount}
      />

      <div className="flex gap-2.5 border-t border-default pt-5">
        <button
          type="button"
          onClick={() => navigate('/categories')}
          className="flex items-center gap-2 rounded-lg bg-accent px-5 py-2.5 text-[14px] font-medium text-white transition-opacity hover:opacity-90"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="19" y1="12" x2="5" y2="12" />
            <polyline points="12 19 5 12 12 5" />
          </svg>
          Back to categories
        </button>
        <button
          type="button"
          onClick={() => start.mutate({ id: result.categoryId, name: result.categoryName })}
          disabled={start.isPending}
          aria-busy={start.isPending}
          className="flex items-center gap-2 rounded-lg border border-strong px-5 py-2.5 text-[14px] font-medium transition-colors hover:bg-elevated disabled:cursor-not-allowed disabled:opacity-60"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="1 4 1 10 7 10" />
            <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
          </svg>
          {start.isPending ? 'Starting…' : 'Try again'}
        </button>
      </div>
    </>
  )
}

function ScoreHero({
  score,
  percentage,
  previousPercentage,
}: {
  score: number
  percentage: number
  previousPercentage: number | null
}) {
  const delta = previousPercentage === null ? null : Math.round(percentage - previousPercentage)

  return (
    <div
      className="mb-4 mt-5 flex items-center justify-between rounded-[14px] border px-8 py-7"
      style={{
        borderColor: 'rgba(139,92,246,0.2)',
        background: 'linear-gradient(135deg, rgba(139,92,246,0.08), rgba(139,92,246,0.02))',
      }}
    >
      <div>
        <p className="mb-1 font-mono text-[12px] uppercase tracking-[0.08em] text-secondary">Your score</p>
        <div className="flex items-baseline gap-1">
          <span className="text-[56px] font-bold leading-none tracking-[-0.04em]">{score}</span>
          <span className="text-2xl font-semibold text-accent-text">%</span>
        </div>
        {delta === null ? (
          <p className="mt-2 text-[12px] font-medium text-muted">First attempt</p>
        ) : (
          <p className={`mt-2 text-[12px] font-medium ${delta >= 0 ? 'text-success' : 'text-danger'}`}>
            {delta >= 0 ? '+' : ''}
            {delta}% from your last attempt
          </p>
        )}
      </div>
      <span
        className="rounded-full px-3 py-1.5 font-mono text-[11px] font-medium tracking-[0.04em] text-accent-text"
        style={{ backgroundColor: 'rgba(139,92,246,0.18)' }}
      >
        {bandLabel(score)}
      </span>
    </div>
  )
}

function MetricCard({ label, mono, children }: { label: string; mono?: boolean; children: React.ReactNode }) {
  return (
    <div className="rounded-[10px] border border-default bg-surface p-3.5">
      <p className="mb-1.5 font-mono text-[10px] uppercase tracking-[0.08em] text-muted">{label}</p>
      <p className={`text-[20px] font-semibold tracking-[-0.02em] ${mono ? 'font-mono' : ''}`}>{children}</p>
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
        <h2 className="text-[14px] font-semibold tracking-[-0.01em]">Review questions</h2>
        <div className="flex gap-1.5">
          <CountPill className="text-success" bg="rgba(16,185,129,0.1)">
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
            className="mt-1.5 rounded-lg border border-dashed border-default p-2.5 font-mono text-[12px] text-secondary transition-colors hover:bg-elevated"
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
        <span className="min-w-[30px] font-mono text-[11px] text-secondary">Q{number}</span>
        <p
          className={`flex-1 text-[13px] ${correct ? 'text-secondary' : 'font-medium text-primary'}`}
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
              <span className="font-mono text-[12px] text-muted">No answer</span>
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
              style={{ borderColor: 'var(--accent)', background: 'rgba(139,92,246,0.06)' }}
            >
              <p className="mb-1 font-mono text-[11px] uppercase tracking-[0.06em] text-secondary">Explanation</p>
              <p className="text-[13px] leading-relaxed text-secondary">{question.explanation}</p>
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
      className={`rounded-full px-2 py-[3px] font-mono text-[10px] font-medium ${className}`}
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
