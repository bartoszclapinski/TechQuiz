import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Area, AreaChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { AchievementsSection } from '../achievements/achievements-section'
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
  // at all, so show the first-run prompt instead of empty charts. A narrower range that happens to be
  // empty falls through to the populated layout (empty charts).
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
  const lastAttempt = summary.recentActivity[0]

  return (
    <main className="mx-auto max-w-[1560px] px-4 py-8 sm:px-6 lg:px-12">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-display text-[clamp(28px,3vw,40px)] font-extrabold leading-[1.05] tracking-[-0.02em]">
            Nice work{name ? `, ${name}` : ''} 👏
          </h1>
          {lastAttempt ? (
            <p className="mt-1.5 text-[16px] text-secondary">
              Last attempt:{' '}
              <span className="font-semibold text-amber-text">
                {Math.round(lastAttempt.scorePercentage)}%
              </span>{' '}
              in {lastAttempt.category}, {relativeTime(lastAttempt.completedAt)}.
            </p>
          ) : (
            <p className="mt-1.5 text-[16px] text-secondary">Here’s how your climb is going.</p>
          )}
        </div>
        <RangeTabs value={range} onChange={onRangeChange} />
      </div>

      <DailyReviewBanner />

      {/* Top bento: a wide score hero (spanning two columns) beside streak + volume. */}
      <div className="mb-3.5 grid gap-3.5 sm:grid-cols-2 lg:grid-cols-4">
        <HeroScoreCard
          average={Math.round(summary.averageScore ?? 0)}
          total={summary.totalQuestionsAnswered}
          points={summary.scoreOverTime}
        />
        <StreakCard days={summary.currentStreakDays} sparkline={summary.activitySparkline} />
        <QuestionsCard total={summary.totalQuestionsAnswered} />
      </div>

      {/* Bottom bento: three equal cards. */}
      <div className="grid gap-3.5 lg:grid-cols-3">
        <CategoryProgressCard categories={summary.categoryStrength} />
        <WeeklyActivityCard sparkline={summary.activitySparkline} />
        <RecentAttemptsCard items={summary.recentActivity} />
      </div>

      <ReviewStatsCard />

      <AchievementsSection />
    </main>
  )
}

function Card({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={`rounded-[22px] border border-default bg-surface ${className}`}>{children}</div>
  )
}

// Uppercase mono micro-label. Amber tone is reserved for gamification signals (streak, score).
function Kicker({ children, tone = 'muted' }: { children: React.ReactNode; tone?: 'muted' | 'amber' }) {
  return (
    <p
      className={`font-mono text-[11px] font-semibold uppercase tracking-[0.12em] ${
        tone === 'amber' ? 'text-amber-text' : 'text-muted'
      }`}
    >
      {children}
    </p>
  )
}

// Radial glow behind hero cards — purely decorative, low alpha, theme-tinted.
function HeroGlow() {
  return (
    <div
      aria-hidden="true"
      className="pointer-events-none absolute -right-10 -top-16 h-60 w-60 rounded-full"
      style={{ background: 'radial-gradient(circle, var(--hero-glow-1), transparent 64%)' }}
    />
  )
}

function HeroScoreCard({
  average,
  total,
  points,
}: {
  average: number
  total: number
  points: DashboardSummary['scoreOverTime']
}) {
  const data = points.map((point) => ({
    label: dateLabel(point.completedAt),
    score: Math.round(point.scorePercentage),
  }))
  const trend =
    points.length >= 2
      ? Math.round(points[points.length - 1].scorePercentage - points[0].scorePercentage)
      : null

  return (
    <div className="relative overflow-hidden rounded-[22px] border border-strong bg-card-grad p-6 sm:col-span-2 lg:col-span-2">
      <HeroGlow />
      <Kicker tone="amber">Average score</Kicker>
      <div className="mt-4 flex items-baseline gap-3">
        <span className="font-display text-[clamp(48px,6vw,72px)] font-extrabold leading-[0.9] tracking-[-0.03em] text-primary">
          {average}
          <span className="text-[0.42em] text-secondary">%</span>
        </span>
        {trend !== null ? (
          <span
            className="text-[15px] font-semibold"
            style={{ color: trend >= 0 ? '#4ade80' : 'var(--warning)' }}
          >
            {trend >= 0 ? '▲' : '▼'} {Math.abs(trend)}% this range
          </span>
        ) : null}
      </div>
      <p className="mt-2 text-[15px] text-secondary">across {total} questions answered</p>
      {data.length >= 2 ? (
        <div className="mt-4">
          <ResponsiveContainer width="100%" height={110}>
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
        </div>
      ) : null}
    </div>
  )
}

function StreakCard({ days, sparkline }: { days: number; sparkline: number[] }) {
  const activeDays = sparkline.filter((value) => value > 0).length
  return (
    <Card className="p-6">
      <Kicker>Streak 🔥</Kicker>
      <div className="mt-3.5 flex items-baseline gap-2">
        <span className="font-display text-[clamp(38px,4vw,52px)] font-extrabold leading-[0.9] text-primary">
          {days}
        </span>
        <span className="text-[15px] text-secondary">{days === 1 ? 'day' : 'days'}</span>
      </div>
      <p className="mt-3.5 text-[14px] text-amber-text">
        {activeDays} active {activeDays === 1 ? 'day' : 'days'} · last {sparkline.length}
      </p>
    </Card>
  )
}

function QuestionsCard({ total }: { total: number }) {
  return (
    <Card className="p-6">
      <Kicker>Questions</Kicker>
      <div className="mt-3.5 flex items-baseline gap-2">
        <span className="font-display text-[clamp(38px,4vw,52px)] font-extrabold leading-[0.9] text-primary">
          {total}
        </span>
      </div>
      <p className="mt-3.5 text-[14px] text-secondary">answered this range</p>
    </Card>
  )
}

function CategoryProgressCard({ categories }: { categories: CategoryStrength[] }) {
  const rows = categories.slice(0, 6)
  return (
    <Card className="p-6">
      <div className="mb-5 flex items-center justify-between">
        <Kicker>Category progress</Kicker>
        <Link to="/categories" className="text-[14px] text-accent hover:underline">
          All →
        </Link>
      </div>
      {rows.length === 0 ? (
        <p className="text-[14px] text-muted">No category data yet.</p>
      ) : (
        <div className="flex flex-col gap-[18px]">
          {rows.map((category) => {
            const pct = Math.round(category.averageScore)
            return (
              <div key={category.category} className="flex items-center gap-3.5">
                <div className="flex h-[38px] w-[38px] shrink-0 items-center justify-center rounded-[11px] bg-elevated font-mono text-[12px] font-bold text-accent">
                  {categoryBadge(category.category)}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="mb-1.5 flex items-center justify-between gap-2">
                    <span className="truncate text-[15px] font-medium text-primary">
                      {category.category}
                    </span>
                    <span className="font-mono text-[14px] font-semibold text-amber-text">{pct}%</span>
                  </div>
                  <ProgressBar pct={pct} />
                </div>
              </div>
            )
          })}
        </div>
      )}
    </Card>
  )
}

function ProgressBar({ pct }: { pct: number }) {
  return (
    <div className="h-2 overflow-hidden rounded-pill bg-track">
      <div className="h-full rounded-pill bg-brand" style={{ width: `${Math.max(0, Math.min(100, pct))}%` }} />
    </div>
  )
}

function WeeklyActivityCard({ sparkline }: { sparkline: number[] }) {
  const week = sparkline.slice(-7)
  const labels = weekdayLabels(week.length)
  const max = Math.max(1, ...week)

  return (
    <Card className="p-6">
      <Kicker>Weekly activity</Kicker>
      <div className="mb-3.5 mt-5 flex h-[150px] items-end gap-2.5">
        {week.map((value, index) => {
          const height = value === 0 ? 4 : Math.max(12, Math.round((value / max) * 100))
          return (
            <div key={index} className="flex h-full flex-1 flex-col justify-end">
              <div
                className={`rounded-lg ${value > 0 ? 'bg-brand' : 'bg-elevated'}`}
                style={{ height: `${height}%` }}
                title={`${value} ${value === 1 ? 'quiz' : 'quizzes'}`}
              />
            </div>
          )
        })}
      </div>
      <div className="flex gap-2.5 font-mono text-[12px] text-muted">
        {labels.map((letter, index) => (
          <span key={index} className="flex-1 text-center">
            {letter}
          </span>
        ))}
      </div>
    </Card>
  )
}

function RecentAttemptsCard({ items }: { items: RecentActivityItem[] }) {
  const rows = items.slice(0, 4)
  return (
    <Card className="p-6">
      <div className="mb-5 flex items-center justify-between">
        <Kicker>Recent attempts</Kicker>
        <Link to="/history" className="text-[14px] text-accent hover:underline">
          History →
        </Link>
      </div>
      {rows.length === 0 ? (
        <p className="text-[14px] text-muted">No attempts yet.</p>
      ) : (
        <div className="flex flex-col gap-3">
          {rows.map((item) => (
            <Link
              key={item.attemptId}
              to={`/result/${item.attemptId}`}
              className="flex items-center gap-3.5 border-b border-default pb-3 last:border-b-0 last:pb-0 hover:opacity-80"
            >
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-[10px] bg-elevated font-mono text-[12px] font-bold text-accent">
                {categoryBadge(item.category)}
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-[15px] font-medium text-primary">{item.category}</p>
                <p className="mt-0.5 font-mono text-[12px] text-muted">
                  {relativeTime(item.completedAt)}
                </p>
              </div>
              <span className={`font-display text-[18px] font-extrabold ${scoreColor(item.scorePercentage)}`}>
                {Math.round(item.scorePercentage)}%
              </span>
            </Link>
          ))}
        </div>
      )}
    </Card>
  )
}

// Review-specific stats — kept apart from quiz aggregates (per the owner's ask). Renders only once
// the user has completed at least one review; otherwise the banner above is the nudge.
function ReviewStatsCard() {
  const { data: stats } = useReviewStats()

  if (!stats || stats.totalSessions === 0) {
    return null
  }

  const accuracy = stats.accuracyPercentage === null ? '—' : Math.round(stats.accuracyPercentage)

  return (
    <Card className="mt-3.5 p-6">
      <div className="mb-4 flex items-center gap-2">
        <ReviewIcon />
        <Kicker>Daily review</Kicker>
      </div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <ReviewMetric
          value={stats.currentStreakDays}
          unit={stats.currentStreakDays === 1 ? 'day' : 'days'}
          label="Review streak"
        />
        <ReviewMetric
          value={stats.bestStreakDays}
          unit={stats.bestStreakDays === 1 ? 'day' : 'days'}
          label="Best streak"
        />
        <ReviewMetric value={stats.totalQuestionsReviewed} label="Questions reviewed" />
        <ReviewMetric
          value={accuracy}
          suffix={accuracy === '—' ? undefined : '%'}
          label="Accuracy"
        />
      </div>
    </Card>
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
        <span className="font-display text-[28px] font-extrabold leading-none tracking-tight text-primary">
          {value}
          {suffix ? <span className="text-base text-secondary">{suffix}</span> : null}
        </span>
        {unit ? <span className="text-[13px] font-medium text-secondary">{unit}</span> : null}
      </div>
      <p className="mt-2 font-mono text-[12px] uppercase tracking-[0.1em] text-muted">{label}</p>
    </div>
  )
}

function ReviewIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="var(--amber-text)" strokeWidth="2" aria-hidden="true">
      <path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8" />
      <polyline points="3 3 3 8 8 8" />
    </svg>
  )
}

// Surfaces the spaced-repetition queue as the primary daily action. Shares the `useDailyReview`
// query with the /review runner (one key), so opening the review from here needs no extra fetch.
function DailyReviewBanner() {
  const { data: questions, isLoading, isError } = useDailyReview()
  const { data: stats } = useReviewStats()

  if (isLoading || isError || !questions) {
    return null
  }

  const count = questions.length

  if (stats?.reviewedToday) {
    return <ReviewedTodayBanner streak={stats.currentStreakDays} remaining={count} />
  }

  if (count === 0) {
    return (
      <div className="mb-3.5 flex items-center gap-2.5 rounded-[18px] border border-default bg-surface px-5 py-3.5 text-[14px] text-secondary">
        <span
          className="flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full"
          style={{ backgroundColor: 'rgba(34,197,94,0.12)' }}
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
    <div className="mb-3.5 flex flex-col gap-3 rounded-[18px] border border-strong bg-card-grad px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-center gap-3.5">
        <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-[13px] bg-amber-bg text-[20px]">
          🔁
        </span>
        <div>
          <p className="text-[16px] font-semibold text-primary">
            {count} {count === 1 ? 'question' : 'questions'} to review today
          </p>
          <p className="mt-0.5 text-[13px] text-secondary">
            Revisit questions you&apos;ve seen before to reinforce what you&apos;ve learned.
          </p>
        </div>
      </div>
      <Link
        to="/review"
        className="flex shrink-0 items-center justify-center gap-1.5 rounded-pill bg-btn px-5 py-2.5 text-[15px] font-semibold text-white shadow-float hover:opacity-90"
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

function ReviewedTodayBanner({ streak, remaining }: { streak: number; remaining: number }) {
  return (
    <div
      className="mb-3.5 flex flex-col gap-3 rounded-[18px] border px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
      style={{ borderColor: 'rgba(34,197,94,0.25)', background: 'rgba(34,197,94,0.06)' }}
    >
      <div className="flex items-center gap-3.5">
        <span
          className="flex h-11 w-11 shrink-0 items-center justify-center rounded-[13px]"
          style={{ backgroundColor: 'rgba(34,197,94,0.12)' }}
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--success)" strokeWidth="2.5" aria-hidden="true">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </span>
        <div>
          <p className="text-[16px] font-semibold text-primary">Reviewed today</p>
          <p className="mt-0.5 text-[13px] text-secondary">
            {streak > 0
              ? `You're on a ${streak}-day review streak — keep it going tomorrow.`
              : 'Nice work clearing your review for today.'}
          </p>
        </div>
      </div>
      {remaining > 0 ? (
        <Link
          to="/review"
          className="flex shrink-0 items-center justify-center gap-1.5 rounded-pill border border-strong px-5 py-2.5 text-[15px] font-medium text-secondary hover:bg-elevated hover:text-primary"
        >
          Review {remaining} more
        </Link>
      ) : null}
    </div>
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
    <main className="mx-auto max-w-[1560px] px-4 py-8 sm:px-6 lg:px-12">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-display text-[clamp(28px,3vw,40px)] font-extrabold leading-[1.05] tracking-[-0.02em]">
            Welcome{name ? `, ${name}` : ''} 👋
          </h1>
          <p className="mt-1.5 text-[16px] text-secondary">
            Take your first quiz to start your climb.
          </p>
        </div>
        <RangeTabs value={range} onChange={onRangeChange} />
      </div>

      <div className="grid gap-3.5 sm:grid-cols-2 lg:grid-cols-4">
        <div className="relative flex flex-col items-center justify-center overflow-hidden rounded-[22px] border border-strong bg-card-grad px-6 py-12 text-center sm:col-span-2 lg:col-span-2">
          <HeroGlow />
          <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-[15px] bg-amber-bg text-[26px]">
            📈
          </div>
          <h3 className="mb-1.5 font-display text-[20px] font-bold text-primary">No data yet</h3>
          <p className="mb-5 max-w-[300px] text-[14px] text-secondary">
            Complete your first quiz to see your score over time, category strength, and streaks here.
          </p>
          <Link
            to="/categories"
            className="flex items-center gap-1.5 rounded-pill bg-btn px-5 py-2.5 text-[15px] font-semibold text-white shadow-float hover:opacity-90"
          >
            Take your first quiz
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" aria-hidden="true">
              <line x1="5" y1="12" x2="19" y2="12" />
              <polyline points="12 5 19 12 12 19" />
            </svg>
          </Link>
        </div>

        <Card className="p-6 opacity-50">
          <Kicker>Streak 🔥</Kicker>
          <span className="mt-3.5 block font-display text-[clamp(38px,4vw,52px)] font-extrabold leading-[0.9] text-muted">
            —
          </span>
        </Card>
        <Card className="p-6 opacity-50">
          <Kicker>Questions</Kicker>
          <span className="mt-3.5 block font-display text-[clamp(38px,4vw,52px)] font-extrabold leading-[0.9] text-muted">
            —
          </span>
        </Card>
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
      className="flex gap-0.5 self-start rounded-pill border border-default bg-base p-[3px]"
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
            className={`rounded-pill px-3.5 py-1 font-mono text-[13px] font-medium transition-colors ${
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

function Centered({ children, tone }: { children: React.ReactNode; tone?: 'danger' }) {
  return (
    <main className="mx-auto max-w-[1560px] px-4 py-8 sm:px-6 lg:px-12">
      <p className={`text-[15px] ${tone === 'danger' ? 'text-danger' : 'text-secondary'}`}>
        {children}
      </p>
    </main>
  )
}

function scoreColor(score: number): string {
  if (score >= 80) return 'text-success'
  if (score >= 70) return 'text-accent-text'
  return 'text-amber-text'
}

// A short badge derived from the category name (e.g. "SQL Basics" → "SQL"). Display-only; the API
// has no icon code for categories, so we synthesise one from the first word.
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

// Weekday initials for the last `count` days, oldest → newest (newest = today).
function weekdayLabels(count: number): string[] {
  const letters = ['S', 'M', 'T', 'W', 'T', 'F', 'S']
  const today = new Date()
  const out: string[] = []
  for (let i = count - 1; i >= 0; i--) {
    const day = new Date(today)
    day.setDate(today.getDate() - i)
    out.push(letters[day.getDay()])
  }
  return out
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
