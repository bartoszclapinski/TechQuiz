import { usePooledQuestions } from './use-pooled-questions'
import type { PooledQuestion } from './api'

// Difficulty arrives as a name (the pool contract is string-typed). Badge colors follow ADR-015
// (emerald/amber/red at ~10% opacity), matching the generate preview.
const DIFFICULTY_BADGE: Record<string, { text: string; bg: string }> = {
  Easy: { text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  Medium: { text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  Hard: { text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
}

export function PoolPage() {
  const { data: questions, isLoading, isError } = usePooledQuestions()

  return (
    <main className="mx-auto max-w-3xl px-6 py-8 sm:px-9">
      <div className="mb-7">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Question pool</h1>
        <p className="text-[13px] text-secondary">
          Questions the community has generated and published. Publish your own drafts from the
          Generate screen.
        </p>
      </div>

      {isLoading ? (
        <p className="text-sm text-secondary">Loading pool…</p>
      ) : isError ? (
        <p className="text-sm text-danger">Couldn’t load the pool. Please try again.</p>
      ) : !questions || questions.length === 0 ? (
        <div className="rounded-[10px] border border-default bg-surface p-4">
          <p className="mb-1 text-[13px] font-semibold text-primary">The pool is empty</p>
          <p className="text-[13px] text-secondary">
            Be the first to publish — generate questions and save them to the pool.
          </p>
        </div>
      ) : (
        <div className="flex flex-col gap-2.5">
          {questions.map((question) => (
            <PoolCard key={question.id} question={question} />
          ))}
        </div>
      )}
    </main>
  )
}

function PoolCard({ question }: { question: PooledQuestion }) {
  const badge = DIFFICULTY_BADGE[question.difficulty]

  return (
    <article className="rounded-[10px] border border-default bg-surface p-3.5">
      <div className="mb-2 flex items-start justify-between gap-3">
        <p className="text-[13px] font-semibold text-primary">{question.stem}</p>
        {badge ? (
          <span
            className={`shrink-0 rounded-full px-2 py-0.5 font-mono text-[10px] ${badge.text}`}
            style={{ backgroundColor: badge.bg }}
          >
            {question.difficulty}
          </span>
        ) : null}
      </div>
      <ul className="mb-2 flex flex-col gap-1">
        {question.options.map((option, optionIndex) => (
          <li key={optionIndex} className="text-[12px] text-secondary">
            <span className="mr-1.5 font-mono text-[10px] text-muted">
              {String.fromCharCode(65 + optionIndex)}.
            </span>
            {option}
          </li>
        ))}
      </ul>
      {question.explanation ? (
        <p className="mb-2 text-[11px] leading-snug text-muted">{question.explanation}</p>
      ) : null}
      <div className="flex items-center gap-2 text-[10px] text-muted">
        <span className="rounded-full bg-elevated px-1.5 py-px font-mono">{question.provider}</span>
        <span>·</span>
        <span>{question.topic}</span>
      </div>
    </article>
  )
}
