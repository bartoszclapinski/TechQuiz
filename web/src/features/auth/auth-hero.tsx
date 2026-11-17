// Decorative showcase panel on the right half of the auth split-screen (Momentum handoff). The
// streak/score tiles are illustrative sample data; the "What's inside" numbers are real seed counts.
// Hidden below lg. The
// gradient glows and avatar swatches use literal hues because the design tokens carry no alpha
// channel; acceptable for a static showcase panel.
const STATS = [
  { value: '🔥 12', label: 'day streak' },
  { value: '182', label: 'Skill IQ' },
  { value: '84%', label: 'accuracy' },
]

// Facts about the product itself (seed data), shown instead of user testimonials or user counts.
const INSIDE = [
  { value: '269', label: 'questions' },
  { value: '18', label: 'topics' },
  { value: '4', label: 'learning tracks' },
]

export function AuthHero() {
  return (
    <div className="relative hidden overflow-hidden bg-elevated lg:block">
      <div
        className="pointer-events-none absolute -right-40 -top-40 h-[640px] w-[640px]"
        style={{ background: 'radial-gradient(circle, var(--hero-glow-1), transparent 62%)' }}
      />
      <div
        className="pointer-events-none absolute -bottom-52 -left-44 h-[640px] w-[640px]"
        style={{ background: 'radial-gradient(circle, var(--hero-glow-2), transparent 62%)' }}
      />

      <div className="relative z-10 mr-auto flex h-full max-w-[520px] flex-col justify-center p-12 lg:ml-28">
        <p className="mb-4 font-mono text-[13px] uppercase tracking-[0.14em] text-muted">
          Sharpen your skills
        </p>
        <h2 className="mb-3 font-display text-[clamp(30px,2.8vw,44px)] font-extrabold leading-[1.1] tracking-[-0.02em]">
          AI-generated quizzes that adapt to how you learn.
        </h2>
        <p className="mb-8 text-[16px] leading-[1.6] text-secondary">
          Pick up exactly where you left off. Your progress, streaks and Skill&nbsp;IQ are waiting.
        </p>

        {/* Highest-score card — gentle float. */}
        <div className="auth-float-hero mb-3.5 rounded-[20px] border border-default bg-surface p-6 shadow-float">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-[46px] w-[46px] items-center justify-center rounded-[13px] bg-brand font-display text-[17px] font-extrabold text-brandfg">
                EF
              </div>
              <div>
                <p className="font-mono text-[12px] uppercase tracking-[0.08em] text-accent-text">
                  Highest score
                </p>
                <p className="font-display text-[17px] font-bold">Entity Framework</p>
              </div>
            </div>
            <span className="font-mono text-[22px] font-bold tracking-tight">94%</span>
          </div>
          <div className="h-[9px] overflow-hidden rounded-pill bg-track">
            <div className="h-full rounded-pill bg-brand" style={{ width: '94%' }} />
          </div>
        </div>

        {/* Stat tiles. */}
        <div className="auth-float-stack mb-6 flex gap-3">
          {STATS.map((stat) => (
            <div
              key={stat.label}
              className="flex-1 rounded-[16px] border border-default bg-surface px-4 py-4"
            >
              <div className="font-display text-[24px] font-extrabold">{stat.value}</div>
              <div className="mt-0.5 text-[13px] text-muted">{stat.label}</div>
            </div>
          ))}
        </div>

        {/* What is inside — real numbers from the seeded question bank, no invented users. */}
        <div className="mb-6 rounded-[18px] border border-default bg-surface px-6 py-5">
          <p className="mb-3 font-mono text-[12px] uppercase tracking-[0.08em] text-accent-text">
            What's inside
          </p>
          <div className="flex gap-6">
            {INSIDE.map((item) => (
              <div key={item.label}>
                <div className="font-display text-[22px] font-extrabold">{item.value}</div>
                <div className="mt-0.5 text-[13px] text-muted">{item.label}</div>
              </div>
            ))}
          </div>
        </div>

        <p className="text-[14px] text-secondary">
          No sign-up needed: try it with the <b className="text-primary">one-click demo account</b>.
        </p>
      </div>
    </div>
  )
}
