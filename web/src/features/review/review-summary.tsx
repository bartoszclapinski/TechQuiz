import { useState } from 'react'
import type { DifficultyValue, ReviewOption } from './api'

// The normalized shape both the post-grade summary and a past-session detail render from. The runner
// builds these from its questions + grade results; the detail view gets them straight off the API
// (ReviewSessionItem is structurally assignable — its extra `category` is ignored here).
export type ReviewResultItem = {
  questionId: string
  questionText: string
  difficulty: DifficultyValue
  options: ReviewOption[]
  selectedOptionId: string | null
  correctOptionId: string
  isCorrect: boolean
  explanation: string
}

// Shared results layout: a score card + a per-question breakdown, used by the live post-grade summary
// and by any re-opened past session. `footer` holds the page-specific navigation.
export function ReviewResultView({
  eyebrow,
  title,
  items,
  footer,
}: {
  eyebrow: string
  title: string
  items: ReviewResultItem[]
  footer: React.ReactNode
}) {
  const correctCount = items.filter((item) => item.isCorrect).length
  const total = items.length
  const score = total > 0 ? Math.round((correctCount / total) * 100) : 0

  return (
    <main className="mx-auto max-w-[800px] px-6 py-8 sm:px-9">
      <div className="mb-2">
        <p className="mb-1.5 font-mono text-[13px] uppercase tracking-[0.1em] text-secondary">{eyebrow}</p>
        <h1 className="text-2xl font-semibold leading-tight tracking-tight">{title}</h1>
      </div>

      <div
        className="mb-6 mt-5 flex items-center justify-between rounded-[14px] border px-8 py-7"
        style={{
          borderColor: 'rgba(139,92,246,0.2)',
          background: 'linear-gradient(135deg, rgba(139,92,246,0.08), rgba(139,92,246,0.02))',
        }}
      >
        <div>
          <p className="mb-1 font-mono text-[13px] uppercase tracking-[0.08em] text-secondary">You got</p>
          <div className="flex items-baseline gap-1">
            <span className="text-[56px] font-bold leading-none tracking-[-0.04em]">{correctCount}</span>
            <span className="text-2xl font-semibold text-accent-text">/ {total}</span>
          </div>
          <p className="mt-2 text-[13px] font-medium text-muted">{score}% correct</p>
        </div>
        <span
          className="rounded-full px-3 py-1.5 font-mono text-[13px] font-medium tracking-[0.04em] text-accent-text"
          style={{ backgroundColor: 'rgba(139,92,246,0.18)' }}
        >
          {bandLabel(score)}
        </span>
      </div>

      <section className="mb-7 flex flex-col gap-1.5">
        {items.map((item, index) => (
          <SummaryRow key={item.questionId} number={index + 1} item={item} />
        ))}
      </section>

      <div className="flex gap-2.5 border-t border-default pt-5">{footer}</div>
    </main>
  )
}

function SummaryRow({ number, item }: { number: number; item: ReviewResultItem }) {
  const [expanded, setExpanded] = useState(!item.isCorrect)
  const correct = item.isCorrect
  const userOption = item.options.find((option) => option.id === item.selectedOptionId)
  const correctOption = item.options.find((option) => option.id === item.correctOptionId)

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
        <span className="min-w-[30px] font-mono text-[13px] text-secondary">Q{number}</span>
        <p className={`flex-1 text-[14px] ${correct ? 'text-secondary' : 'font-medium text-primary'}`}>
          {item.questionText}
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
          {item.explanation && (
            <div
              className="mt-1 rounded-r border-l-2 px-3 py-2.5"
              style={{ borderColor: 'var(--accent)', background: 'rgba(139,92,246,0.06)' }}
            >
              <p className="mb-1 font-mono text-[13px] uppercase tracking-[0.06em] text-secondary">Explanation</p>
              <p className="text-[14px] leading-relaxed text-secondary">{item.explanation}</p>
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
  const bg = tone === 'success' ? 'rgba(16,185,129,0.08)' : 'rgba(239,68,68,0.08)'
  return (
    <code className={`rounded px-2 py-[3px] font-mono text-[13px] ${color}`} style={{ backgroundColor: bg }}>
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

function bandLabel(score: number): string {
  if (score >= 80) return 'Great work'
  if (score >= 60) return 'Good effort'
  if (score >= 40) return 'Keep practicing'
  return 'Keep going'
}
