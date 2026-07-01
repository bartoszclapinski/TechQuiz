import type { Achievement } from './api'
import { AchievementIcon } from './icons'
import { useAchievements } from './use-achievements'

// The achievements section on the Dashboard: a header roll-up (N/M unlocked) and a badge grid.
// Unlocked badges render in accent; locked ones are greyed with a progress hint (and a bar for
// multi-step badges). Renders nothing while loading or on error — it's a supplement, not load-bearing.
export function AchievementsSection() {
  const { data, isLoading, isError } = useAchievements()

  if (isLoading || isError || !data) {
    return null
  }

  return (
    <section className="mt-2.5">
      <div className="mb-3 flex items-center justify-between px-0.5">
        <h2 className="font-mono text-[11px] uppercase tracking-[0.08em] text-muted">Achievements</h2>
        <span className="font-mono text-[11px] text-secondary">
          {data.unlockedCount}/{data.totalCount} unlocked
        </span>
      </div>
      <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2 lg:grid-cols-3">
        {data.items.map((badge) => (
          <BadgeCard key={badge.key} badge={badge} />
        ))}
      </div>
    </section>
  )
}

function BadgeCard({ badge }: { badge: Achievement }) {
  const { unlocked, title, description, group, progress, target } = badge
  const multiStep = target > 1

  return (
    <div
      className={`flex items-start gap-3 rounded-xl border bg-surface p-[18px] ${
        unlocked ? 'border-accent/30' : 'border-default'
      }`}
    >
      <span
        className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${
          unlocked ? 'bg-accent-bg text-accent-text' : 'bg-elevated text-muted'
        }`}
        aria-hidden="true"
      >
        <AchievementIcon group={group} />
      </span>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className={`text-[14px] font-semibold ${unlocked ? 'text-primary' : 'text-secondary'}`}>
            {title}
          </p>
          {unlocked ? <UnlockedCheck /> : null}
        </div>
        <p className={`mt-0.5 text-[12px] ${unlocked ? 'text-secondary' : 'text-muted'}`}>
          {description}
        </p>
        {!unlocked && multiStep ? <ProgressBar progress={progress} target={target} /> : null}
      </div>
    </div>
  )
}

function ProgressBar({ progress, target }: { progress: number; target: number }) {
  const pct = target > 0 ? Math.min(100, Math.round((progress / target) * 100)) : 0

  return (
    <div className="mt-2.5">
      <div className="h-1.5 overflow-hidden rounded-full bg-elevated">
        <div className="h-full rounded-full bg-accent" style={{ width: `${pct}%` }} />
      </div>
      <p className="mt-1 font-mono text-[10px] text-muted">
        {progress}/{target}
      </p>
    </div>
  )
}

function UnlockedCheck() {
  return (
    <span
      className="flex h-4 w-4 shrink-0 items-center justify-center rounded-full"
      style={{ backgroundColor: 'rgba(16,185,129,0.15)' }}
      aria-label="Unlocked"
    >
      <svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="3.5" aria-hidden="true">
        <polyline points="20 6 9 17 4 12" />
      </svg>
    </span>
  )
}
