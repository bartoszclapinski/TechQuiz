import { Link } from 'react-router-dom'
import { useCodeChallenges } from './use-code-challenges'
import type { CodeChallenge } from './api'

// Difficulty arrives as a name. Badge colors follow ADR-015 (emerald/amber/red at ~10%),
// matching the pool and generate screens.
const DIFFICULTY_BADGE: Record<string, { text: string; bg: string }> = {
  Easy: { text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  Medium: { text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  Hard: { text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
}

export function CodeChallengesPage() {
  const { data: challenges, isLoading, isError } = useCodeChallenges()

  return (
    <main className="mx-auto max-w-3xl px-6 py-8 sm:px-9">
      <div className="mb-7">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Code challenges</h1>
        <p className="text-[14px] text-secondary">
          Write C#, run it, and submit for grading against hidden tests. Your code compiles and
          runs in an isolated sandbox.
        </p>
      </div>

      {isLoading ? (
        <p className="text-[15px] text-secondary">Loading challenges…</p>
      ) : isError ? (
        <p className="text-[15px] text-danger">Couldn’t load challenges. Please try again.</p>
      ) : !challenges || challenges.length === 0 ? (
        <div className="rounded-[10px] border border-default bg-surface p-4">
          <p className="text-[14px] text-secondary">No challenges available yet.</p>
        </div>
      ) : (
        <div className="flex flex-col gap-2.5">
          {challenges.map((challenge) => (
            <ChallengeCard key={challenge.id} challenge={challenge} />
          ))}
        </div>
      )}
    </main>
  )
}

function ChallengeCard({ challenge }: { challenge: CodeChallenge }) {
  const badge = DIFFICULTY_BADGE[challenge.difficulty]

  return (
    <Link
      to={`/challenges/${challenge.id}`}
      className="block rounded-[10px] border border-default bg-surface p-3.5 transition-colors hover:border-accent"
    >
      <div className="mb-1.5 flex items-start justify-between gap-3">
        <p className="text-[15px] font-semibold text-primary">{challenge.title}</p>
        {badge ? (
          <span
            className={`shrink-0 rounded-full px-2 py-0.5 font-mono text-[12px] ${badge.text}`}
            style={{ backgroundColor: badge.bg }}
          >
            {challenge.difficulty}
          </span>
        ) : null}
      </div>
      <p className="line-clamp-2 text-[13px] leading-snug text-secondary">{challenge.prompt}</p>
    </Link>
  )
}
