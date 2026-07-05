// Decorative showcase panel on the right half of the auth split-screen. Static sample data —
// it sells the product visually for the portfolio demo and is hidden below the lg breakpoint.
// The gradient blobs and card boxShadow below use literal accent-hued rgba (--accent #8b5cf6,
// indigo #6366f1): the design tokens carry no alpha channel, so these decorations can't derive
// from them and won't track a future --accent change. Acceptable for a static showcase panel.
const SUPPORTING = [
  { code: 'ASP', label: 'ASP.NET Core', pct: 72 },
  { code: 'EF', label: 'Entity Framework', pct: 94 },
  { code: 'SQL', label: 'SQL Basics', pct: 65 },
]

export function AuthHero() {
  return (
    <div className="relative hidden overflow-hidden bg-surface lg:block">
      <div
        className="pointer-events-none absolute -right-32 -top-32 h-[540px] w-[540px]"
        style={{
          background:
            'radial-gradient(circle at center, rgba(139,92,246,0.30) 0%, rgba(139,92,246,0.07) 35%, transparent 70%)',
        }}
      />
      <div
        className="pointer-events-none absolute -bottom-44 -left-20 h-[460px] w-[460px]"
        style={{
          background:
            'radial-gradient(circle at center, rgba(99,102,241,0.20) 0%, rgba(99,102,241,0.03) 40%, transparent 70%)',
        }}
      />
      <div className="auth-grid pointer-events-none absolute inset-0" />

      <div className="relative z-10 flex h-full flex-col p-12">
        <div className="mb-10">
          <p className="mb-3 font-mono text-[13px] uppercase tracking-[0.14em] text-muted">
            Sharpen your skills
          </p>
          <h2 className="max-w-[380px] text-3xl font-bold leading-tight tracking-tight">
            AI-generated quizzes that adapt to how you learn.
          </h2>
        </div>

        <div
          className="auth-float-hero mb-3.5 rounded-2xl border border-default bg-elevated p-6"
          style={{ boxShadow: '0 0 0 1px rgba(139,92,246,0.18)' }}
        >
          <div className="mb-3.5 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-[10px] bg-accent-bg font-mono text-base font-bold text-accent-text">
                C#
              </div>
              <div>
                <p className="mb-0.5 font-mono text-[13px] uppercase tracking-[0.08em] text-accent-text">
                  Last session
                </p>
                <p className="text-[17px] font-semibold">C# Advanced</p>
              </div>
            </div>
            <span className="font-mono text-[22px] font-bold tracking-tight">87%</span>
          </div>
          <div className="mb-3 h-1.5 overflow-hidden rounded-full bg-surface">
            <div className="h-full rounded-full bg-accent" style={{ width: '87%' }} />
          </div>
          <div className="flex justify-between font-mono text-[13px] text-secondary">
            <span>13 / 15 correct</span>
            <span>4 min 12 s</span>
          </div>
        </div>

        <div className="auth-float-stack flex flex-col gap-2">
          {SUPPORTING.map((row) => (
            <div
              key={row.code}
              className="flex items-center gap-3 rounded-[10px] border border-default bg-elevated px-4 py-3"
            >
              <div className="flex h-7 w-7 items-center justify-center rounded-md bg-accent-bg font-mono text-[12px] font-semibold text-accent-text">
                {row.code}
              </div>
              <p className="flex-1 text-[14px] font-medium">{row.label}</p>
              <div className="h-1 w-20 overflow-hidden rounded-full bg-surface">
                <div className="h-full rounded-full bg-accent" style={{ width: `${row.pct}%` }} />
              </div>
              <span className="min-w-[26px] text-right font-mono text-[13px] font-medium text-secondary">
                {row.pct}%
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
