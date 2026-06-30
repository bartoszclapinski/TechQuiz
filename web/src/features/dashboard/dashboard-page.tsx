import { useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Area,
  AreaChart,
  PolarAngleAxis,
  PolarGrid,
  PolarRadiusAxis,
  Radar,
  RadarChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { useAuth } from '../auth/use-auth'
import { useDailyReview } from '../review/use-daily-review'
import { useReviewStats } from '../review/use-review-stats'
import { useDashboard } from './use-dashboard'
import type { CategoryStrength, DashboardRange, DashboardSummary, RecentActivityItem } from './api'

export function DashboardPage() {
  const [range, setRange] = useState<DashboardRange>('all')
  const { data, isLoading, isError } = useDashboard(range)

  if (isLoading) {
    return <Centered>Loading your dashboard…</Centered>
  }
  if (isError || !data) {
    return <Centered tone="danger">Couldn’t load your dashboard.</Centered>
  }

  // First-run empty state: on the All range a null average means the user has no completed attempts
  // at all, so show the first-run prompt instead of empty charts (matches dashboard-empty-state.html).
  // A narrower range that happens to be empty falls through to the populated layout (empty charts).
  if (range === 'all' && data.averageScore === null) {
    return <EmptyDashboard range={range} onRangeChange={setRange} />
  }

  return <PopulatedDashboard summary={data} range={range} onRangeChange={setRange} />
}

function PopulatedDashboard({
  summary,
  range,
  onRangeChange,
}: {
  summary: DashboardSummary
  range: DashboardRange
  onRangeChange: (range: DashboardRange) => void
}) {
  const { user } = useAuth()
  const name = firstName(user?.email)

  const best = bestCategory(summary.categoryStrength)
  const needsPractice = weakestCategory(summary.categoryStrength)
  const lastAttempt = summary.recentActivity[0]

  return (
    <main className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Welcome back{name ? `, ${name}` : ''}</h1>
          {lastAttempt ? (
            <p className="mt-1 text-[13px] text-secondary">
              Last attempt:{' '}
              <span className="font-medium text-accent-text">{Math.round(lastAttempt.scorePercentage)}%</span>{' '}
              in {lastAttempt.category}, {relativeTime(lastAttempt.completedAt)}.
            </p>
          ) : null}
        </div>
        <RangeTabs value={range} onChange={onRangeChange} />
      </div>

      <DailyReviewBanner />

      <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-3">
        <StreakTile days={summary.currentStreakDays} sparkline={summary.activitySparkline} />
        <ScoreOverTimeTile points={summary.scoreOverTime} />
        <CategoryStrengthTile categories={summary.categoryStrength} />
        <StatTile label="Questions answered" value={String(summary.totalQuestionsAnswered)} />
        <StatTile label="Average score" value={Math.round(summary.averageScore ?? 0)} suffix="%" />
        <RecentActivityTile items={summary.recentActivity} />
        <CategoryExtremeTile label="Best category" category={best} tone="success" />
        <CategoryExtremeTile label="Needs practice" category={needsPractice} tone="warning" />
        <ReviewStatsTile />
      </div>
    </main>
  )
}

// Review-specific stats live in their own tile (kept apart from quiz aggregates, per the owner's
// ask): the daily queue is small, so streak / accuracy / volume read differently here. Renders only
// once the user has completed at least one review — otherwise the banner above is the nudge.
function ReviewStatsTile() {
  const { data: stats } = useReviewStats()

  if (!stats || stats.totalSessions === 0) {
    return null
  }

  const accuracy = stats.accuracyPercentage === null ? '—' : Math.round(stats.accuracyPercentage)

  return (
    <Tile className="p-[18px] sm:col-span-3">
      <div className="mb-3.5 flex items-center gap-2">
        <ReviewIcon />
        <Label>Daily review</Label>
      </div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <ReviewMetric value={stats.currentStreakDays} unit={stats.currentStreakDays === 1 ? 'day' : 'days'} label="Review streak" />
        <ReviewMetric value={stats.bestStreakDays} unit={stats.bestStreakDays === 1 ? 'day' : 'days'} label="Best streak" />
        <ReviewMetric value={stats.totalQuestionsReviewed} label="Questions reviewed" />
        <ReviewMetric value={accuracy} suffix={accuracy === '—' ? undefined : '%'} label="Accuracy" />
      </div>
    </Tile>
  )
}

function ReviewMetric({
  value,
  unit,
  suffix,
  label,
}: {
  value: string | number
  unit?: string
  suffix?: string
  label: string
}) {
  return (
    <div>
      <div className="flex items-baseline gap-1.5">
        <span className="text-[28px] font-bold leading-none tracking-tight text-primary">
          {value}
          {suffix ? <span className="text-base text-secondary">{suffix}</span> : null}
        </span>
        {unit ? <span className="text-xs font-medium text-secondary">{unit}</span> : null}
      </div>
      <p className="mt-2 font-mono text-[11px] uppercase tracking-[0.08em] text-muted">{label}</p>
    </div>
  )
}

function ReviewIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="var(--accent-text)" strokeWidth="2" aria-hidden="true">
      <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
      <polyline points="3 3 3 8 8 8" />
    </svg>
  )
}

// Surfaces the spaced-repetition queue as the primary daily action. Shares the `useDailyReview`
// query with the /review runner (one key), so opening the review from here needs no extra fetch.
// While loading or on error we render nothing — the banner is a nudge, not load-bearing content.
function DailyReviewBanner() {
  const { data: questions, isLoading, isError } = useDailyReview()
  const { data: stats } = useReviewStats()

  if (isLoading || isError || !questions) {
    return null
  }

  const count = questions.length

  // Already reviewed today: switch from a nudge to an acknowledgement (with the streak), so a return
  // visit doesn't keep pushing the same call to action. Still offer a way back in if the queue holds
  // questions the user didn't clear (e.g. ones they got wrong again).
  if (stats?.reviewedToday) {
    return <ReviewedTodayBanner streak={stats.currentStreakDays} remaining={count} />
  }

  if (count === 0) {
    return (
      <div className="mb-2.5 flex items-center gap-2.5 rounded-xl border border-default bg-surface px-[18px] py-3 text-[13px] text-secondary">
        <span
          className="flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full"
          style={{ backgroundColor: 'rgba(16,185,129,0.1)' }}
        >
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="3" aria-hidden="true">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </span>
        You&apos;re all caught up on reviews today.
      </div>
    )
  }

  return (
    <div
      className="mb-2.5 flex flex-col gap-3 rounded-xl border px-[18px] py-4 sm:flex-row sm:items-center sm:justify-between"
      style={{
        borderColor: 'rgba(139,92,246,0.25)',
        background: 'linear-gradient(135deg, rgba(139,92,246,0.1), rgba(139,92,246,0.02))',
      }}
    >
      <div className="flex items-center gap-3">
        <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-accent-bg">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--accent-text)" strokeWidth="2" aria-hidden="true">
            <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
            <polyline points="3 3 3 8 8 8" />
          </svg>
        </span>
        <div>
          <p className="text-[15px] font-semibold text-primary">
            {count} {count === 1 ? 'question' : 'questions'} to review today
          </p>
          <p className="mt-0.5 text-[12px] text-secondary">
            Revisit questions you&apos;ve seen before to reinforce what you&apos;ve learned.
          </p>
        </div>
      </div>
      <Link
        to="/review"
        className="flex shrink-0 items-center justify-center gap-1.5 rounded-lg bg-accent px-4 py-2 text-[13px] font-medium text-white hover:opacity-90"
      >
        Start review
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" aria-hidden="true">
          <line x1="5" y1="12" x2="19" y2="12" />
          <polyline points="12 5 19 12 12 19" />
        </svg>
      </Link>
    </div>
  )
}

// Shown once the user has reviewed today: a green confirmation that also reports the streak the
// session earned. If the queue still has questions, a quiet link lets them keep going.
function ReviewedTodayBanner({ streak, remaining }: { streak: number; remaining: number }) {
  return (
    <div
      className="mb-2.5 flex flex-col gap-3 rounded-xl border px-[18px] py-4 sm:flex-row sm:items-center sm:justify-between"
      style={{ borderColor: 'rgba(16,185,129,0.25)', background: 'rgba(16,185,129,0.06)' }}
    >
      <div className="flex items-center gap-3">
        <span
          className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl"
          style={{ backgroundColor: 'rgba(16,185,129,0.12)' }}
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="2.5" aria-hidden="true">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </span>
        <div>
          <p className="text-[15px] font-semibold text-primary">Reviewed today</p>
          <p className="mt-0.5 text-[12px] text-secondary">
            {streak > 0
              ? `You're on a ${streak}-day review streak — keep it going tomorrow.`
              : 'Nice work clearing your review for today.'}
          </p>
        </div>
      </div>
      {remaining > 0 ? (
        <Link
          to="/review"
          className="flex shrink-0 items-center justify-center gap-1.5 rounded-lg border border-default px-4 py-2 text-[13px] font-medium text-secondary hover:bg-elevated hover:text-primary"
        >
          Review {remaining} more
        </Link>
      ) : null}
    </div>
  )
}

function Tile({
  children,
  className = '',
}: {
  children: React.ReactNode
  className?: string
}) {
  return (
    <div className={`rounded-xl border border-default bg-surface ${className}`}>{children}</div>
  )
}

function Label({ children }: { children: React.ReactNode }) {
  return (
    <span className="font-mono text-[11px] uppercase tracking-[0.08em] text-muted">{children}</span>
  )
}

function StreakTile({ days, sparkline }: { days: number; sparkline: number[] }) {
  return (
    <Tile className="relative overflow-hidden p-[18px]">
      <div className="mb-3.5 flex items-center gap-2">
        <FlameIcon />
        <Label>Current streak</Label>
      </div>
      <div className="mb-3.5 flex items-baseline gap-1.5">
        <span className="text-[40px] font-bold leading-none tracking-tight text-primary">{days}</span>
        <span className="text-sm font-medium text-secondary">{days === 1 ? 'day' : 'days'}</span>
      </div>
      <Sparkline values={sparkline} />
      <p className="mt-1.5 font-mono text-[11px] text-muted">Last {sparkline.length} days</p>
    </Tile>
  )
}

// Inline SVG sparkline — a single sub-100px path is cheaper hand-rolled than via a chart lib.
function Sparkline({ values }: { values: number[] }) {
  const width = 240
  const height = 32
  const pad = 2
  const max = Math.max(1, ...values)
  const step = values.length > 1 ? width / (values.length - 1) : width

  const points = values.map((value, index) => {
    const x = index * step
    const y = height - pad - (value / max) * (height - pad * 2)
    return `${x.toFixed(1)},${y.toFixed(1)}`
  })

  const line = points.map((point, index) => `${index === 0 ? 'M' : 'L'}${point}`).join(' ')
  const area = `${line} L${width},${height} L0,${height} Z`

  return (
    <svg
      width="100%"
      height={height}
      viewBox={`0 0 ${width} ${height}`}
      preserveAspectRatio="none"
      className="block"
      aria-hidden="true"
    >
      <defs>
        <linearGradient id="sparkGrad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="var(--accent)" stopOpacity="0.5" />
          <stop offset="100%" stopColor="var(--accent)" stopOpacity="0" />
        </linearGradient>
      </defs>
      <path d={area} fill="url(#sparkGrad)" />
      <path d={line} stroke="var(--accent)" strokeWidth="1.5" fill="none" />
    </svg>
  )
}

function ScoreOverTimeTile({ points }: { points: DashboardSummary['scoreOverTime'] }) {
  const data = points.map((point) => ({
    label: dateLabel(point.completedAt),
    score: Math.round(point.scorePercentage),
  }))
  const trend =
    points.length >= 2
      ? Math.round(points[points.length - 1].scorePercentage - points[0].scorePercentage)
      : null

  return (
    <Tile className="p-[18px] sm:col-span-2">
      <div className="mb-3.5 flex items-center justify-between">
        <Label>Score over time</Label>
        {trend !== null ? (
          <span
            className={`rounded-full px-2 py-0.5 text-[11px] font-medium ${
              trend >= 0 ? 'text-success' : 'text-warning'
            }`}
            style={{ backgroundColor: trend >= 0 ? 'rgba(16,185,129,0.1)' : 'rgba(245,158,11,0.1)' }}
          >
            {trend >= 0 ? '+' : ''}
            {trend}% trend
          </span>
        ) : null}
      </div>
      <ResponsiveContainer width="100%" height={140}>
        <AreaChart data={data} margin={{ top: 6, right: 8, bottom: 0, left: -20 }}>
          <defs>
            <linearGradient id="scoreGrad" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="var(--accent)" stopOpacity="0.35" />
              <stop offset="100%" stopColor="var(--accent)" stopOpacity="0" />
            </linearGradient>
          </defs>
          <XAxis
            dataKey="label"
            tick={{ fontSize: 10, fill: 'var(--text-muted)' }}
            tickLine={false}
            axisLine={false}
          />
          <YAxis
            domain={[0, 100]}
            tick={{ fontSize: 10, fill: 'var(--text-muted)' }}
            tickLine={false}
            axisLine={false}
            width={40}
          />
          <Tooltip
            cursor={{ stroke: 'var(--border-strong)' }}
            contentStyle={{
              background: 'var(--bg-elevated)',
              border: '1px solid var(--border-default)',
              borderRadius: 8,
              fontSize: 12,
            }}
            labelStyle={{ color: 'var(--text-secondary)' }}
            itemStyle={{ color: 'var(--text-primary)' }}
            formatter={(value) => [`${value}%`, 'Score']}
          />
          <Area
            type="monotone"
            dataKey="score"
            stroke="var(--accent)"
            strokeWidth={2}
            fill="url(#scoreGrad)"
          />
        </AreaChart>
      </ResponsiveContainer>
    </Tile>
  )
}

function CategoryStrengthTile({ categories }: { categories: CategoryStrength[] }) {
  const data = categories.map((category) => ({
    category: category.category,
    score: Math.round(category.averageScore),
  }))

  return (
    <Tile className="p-[18px] sm:row-span-2">
      <Label>Category strength</Label>
      <div className="flex items-center justify-center py-2">
        <ResponsiveContainer width="100%" height={220}>
          <RadarChart data={data} outerRadius="70%">
            <PolarGrid stroke="var(--border-default)" />
            <PolarAngleAxis
              dataKey="category"
              tick={{ fontSize: 10, fill: 'var(--text-secondary)' }}
            />
            <PolarRadiusAxis domain={[0, 100]} tick={false} axisLine={false} />
            <Radar
              dataKey="score"
              stroke="var(--accent)"
              strokeWidth={1.5}
              fill="var(--accent)"
              fillOpacity={0.25}
            />
          </RadarChart>
        </ResponsiveContainer>
      </div>
      <p className="mt-1 text-center font-mono text-[11px] text-muted">
        {categories.length}-category overview
      </p>
    </Tile>
  )
}

function StatTile({
  label,
  value,
  suffix,
}: {
  label: string
  value: string | number
  suffix?: string
}) {
  return (
    <Tile className="px-[18px] py-3.5">
      <p className="mb-2">
        <Label>{label}</Label>
      </p>
      <div className="flex items-baseline gap-1.5">
        <span className="text-[28px] font-bold leading-none tracking-tight text-primary">
          {value}
          {suffix ? <span className="text-base text-secondary">{suffix}</span> : null}
        </span>
      </div>
    </Tile>
  )
}

function RecentActivityTile({ items }: { items: RecentActivityItem[] }) {
  return (
    <Tile className="p-[18px] sm:col-span-2">
      <div className="mb-3 flex items-center justify-between">
        <Label>Recent activity</Label>
      </div>
      <div className="flex flex-col">
        {items.map((item) => (
          <div
            key={item.attemptId}
            className="flex items-center gap-2.5 border-b border-default py-2 last:border-b-0"
          >
            <div className="flex h-[26px] w-[26px] items-center justify-center rounded-md bg-accent-bg font-mono text-[10px] font-semibold text-accent-text">
              {categoryBadge(item.category)}
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-xs font-medium text-primary">{item.category}</p>
              <p className="font-mono text-[10px] text-secondary">{relativeTime(item.completedAt)}</p>
            </div>
            <span className={`font-mono text-[13px] font-bold ${scoreColor(item.scorePercentage)}`}>
              {Math.round(item.scorePercentage)}%
            </span>
          </div>
        ))}
      </div>
    </Tile>
  )
}

function CategoryExtremeTile({
  label,
  category,
  tone,
}: {
  label: string
  category: CategoryStrength | null
  tone: 'success' | 'warning'
}) {
  const toneText = tone === 'success' ? 'text-success' : 'text-warning'
  const toneBg = tone === 'success' ? 'rgba(16,185,129,0.15)' : 'rgba(245,158,11,0.15)'
  const borderClass = tone === 'warning' ? 'border-warning/30' : 'border-default'

  return (
    <Tile className={`px-[18px] py-3.5 ${borderClass}`}>
      <p className="mb-2">
        <Label>{label}</Label>
      </p>
      {category ? (
        <>
          <div className="mb-2 flex items-center gap-2.5">
            <div
              className={`flex h-[26px] w-[26px] items-center justify-center rounded-md font-mono text-[10px] font-semibold ${toneText}`}
              style={{ backgroundColor: toneBg }}
            >
              {categoryBadge(category.category)}
            </div>
            <p className="text-[13px] font-semibold text-primary">{category.category}</p>
          </div>
          <div className="flex items-baseline gap-1.5">
            <span className={`text-[22px] font-bold leading-none tracking-tight ${toneText}`}>
              {Math.round(category.averageScore)}
              <span className="text-xs">%</span>
            </span>
            <span className="font-mono text-[10px] text-muted">
              {category.attemptCount} {category.attemptCount === 1 ? 'attempt' : 'attempts'}
            </span>
          </div>
        </>
      ) : (
        <p className="text-[13px] text-muted">Not enough data</p>
      )}
    </Tile>
  )
}

function EmptyDashboard({
  range,
  onRangeChange,
}: {
  range: DashboardRange
  onRangeChange: (range: DashboardRange) => void
}) {
  const { user } = useAuth()
  const name = firstName(user?.email)

  return (
    <main className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Welcome{name ? `, ${name}` : ''}</h1>
          <p className="mt-1 text-[13px] text-secondary">
            Take your first quiz to start tracking your progress.
          </p>
        </div>
        <RangeTabs value={range} onChange={onRangeChange} />
      </div>

      <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-3">
        <Tile className="p-[18px] opacity-40">
          <div className="mb-3.5 flex items-center gap-2">
            <FlameIcon muted />
            <Label>Current streak</Label>
          </div>
          <span className="text-[40px] font-bold leading-none tracking-tight text-muted">—</span>
        </Tile>

        <Tile className="relative flex flex-col items-center justify-center overflow-hidden px-6 py-8 text-center sm:col-span-2">
          <div className="mb-3.5 flex h-12 w-12 items-center justify-center rounded-xl bg-accent-bg">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="var(--accent-text)" strokeWidth="2" aria-hidden="true">
              <polyline points="22 12 18 12 15 21 9 3 6 12 2 12" />
            </svg>
          </div>
          <h3 className="mb-1.5 text-[15px] font-semibold text-primary">No data yet</h3>
          <p className="mb-4 max-w-[280px] text-xs text-secondary">
            Complete your first quiz to see your score over time, category strength, and learning
            trends here.
          </p>
          <Link
            to="/categories"
            className="flex items-center gap-1.5 rounded-lg bg-accent px-4 py-2 text-[13px] font-medium text-white hover:opacity-90"
          >
            Take your first quiz
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" aria-hidden="true">
              <line x1="5" y1="12" x2="19" y2="12" />
              <polyline points="12 5 19 12 12 19" />
            </svg>
          </Link>
        </Tile>

        <Tile className="px-[18px] py-3.5 opacity-40">
          <p className="mb-2">
            <Label>Questions answered</Label>
          </p>
          <span className="text-[28px] font-bold leading-none tracking-tight text-muted">—</span>
        </Tile>
        <Tile className="px-[18px] py-3.5 opacity-40">
          <p className="mb-2">
            <Label>Average score</Label>
          </p>
          <span className="text-[28px] font-bold leading-none tracking-tight text-muted">—</span>
        </Tile>
        <Tile className="px-[18px] py-3.5 opacity-40">
          <p className="mb-2">
            <Label>Best category</Label>
          </p>
          <p className="text-[13px] text-muted">Not enough data</p>
        </Tile>
      </div>
    </main>
  )
}

const RANGE_OPTIONS: { value: DashboardRange; label: string }[] = [
  { value: 'week', label: 'Week' },
  { value: 'month', label: 'Month' },
  { value: 'all', label: 'All time' },
]

function RangeTabs({
  value,
  onChange,
}: {
  value: DashboardRange
  onChange: (range: DashboardRange) => void
}) {
  return (
    <div
      role="tablist"
      aria-label="Time range"
      className="flex gap-0.5 self-start rounded-lg border border-default bg-base p-[3px]"
    >
      {RANGE_OPTIONS.map((option) => {
        const active = option.value === value
        return (
          <button
            key={option.value}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onChange(option.value)}
            className={`rounded-md px-3 py-1 font-mono text-xs font-medium transition-colors ${
              active ? 'bg-elevated text-primary' : 'text-secondary hover:text-primary'
            }`}
          >
            {option.label}
          </button>
        )
      })}
    </div>
  )
}

function FlameIcon({ muted = false }: { muted?: boolean }) {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 24 24"
      fill="none"
      stroke={muted ? 'var(--text-muted)' : 'var(--warning)'}
      strokeWidth="2"
      aria-hidden="true"
    >
      <path d="M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5z" />
    </svg>
  )
}

function Centered({ children, tone }: { children: React.ReactNode; tone?: 'danger' }) {
  return (
    <main className="mx-auto max-w-6xl px-6 py-8 sm:px-8">
      <p className={`text-sm ${tone === 'danger' ? 'text-danger' : 'text-secondary'}`}>{children}</p>
    </main>
  )
}

function bestCategory(categories: CategoryStrength[]): CategoryStrength | null {
  if (categories.length === 0) return null
  return categories.reduce((best, c) => (c.averageScore > best.averageScore ? c : best))
}

function weakestCategory(categories: CategoryStrength[]): CategoryStrength | null {
  if (categories.length === 0) return null
  return categories.reduce((worst, c) => (c.averageScore < worst.averageScore ? c : worst))
}

function scoreColor(score: number): string {
  if (score >= 80) return 'text-success'
  if (score >= 70) return 'text-accent-text'
  return 'text-warning'
}

// A short badge derived from the category name (e.g. "SQL Basics" → "SQL"). Display-only; the
// API has no icon code for categories, so we synthesise one from the first word.
function categoryBadge(name: string): string {
  const first = name.trim().split(/\s+/)[0] ?? ''
  return (first.length <= 4 ? first : first.slice(0, 3)).toUpperCase()
}

function firstName(email: string | undefined): string {
  if (!email) return ''
  const local = email.split('@')[0] ?? ''
  const head = local.split(/[._-]+/)[0] ?? ''
  return head ? head.charAt(0).toUpperCase() + head.slice(1) : ''
}

function dateLabel(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function relativeTime(iso: string): string {
  const then = new Date(iso).getTime()
  const diffMs = Date.now() - then
  const minutes = Math.floor(diffMs / 60_000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes} ${minutes === 1 ? 'minute' : 'minutes'} ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} ${hours === 1 ? 'hour' : 'hours'} ago`
  const days = Math.floor(hours / 24)
  if (days === 1) return 'yesterday'
  return `${days} days ago`
}
