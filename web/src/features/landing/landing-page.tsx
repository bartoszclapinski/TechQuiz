import { Link } from 'react-router-dom'
import { ThemeToggle } from '../../components/ui/theme-toggle'

// Public marketing entry (Momentum handoff). Static, aspirational copy — no live user data. Metrics
// referenced here (streaks, accuracy, category progress, AI generation, spaced repetition) map to
// real product features; invented XP / Skill IQ numbers from the prototype are intentionally left
// out so the page doesn't overpromise (ADR-024).
const TOPICS = ['C#', 'ASP.NET Core', 'EF Core', 'SQL', 'Design Patterns', 'Unit Testing', 'Git']

const FEATURES = [
  {
    icon: '✨',
    title: 'AI-generated questions',
    body: 'Bring your own key and generate fresh questions on any topic. Every one is saved to a shared pool for the community.',
  },
  {
    icon: '🔁',
    title: 'Spaced repetition',
    body: 'A simplified SM-2 schedule resurfaces exactly what you’re about to forget, so review time is never wasted.',
  },
  {
    icon: '📈',
    title: 'Progress that motivates',
    body: 'Streaks, accuracy and category strength track your climb across every topic and keep the momentum going.',
  },
]

const STEPS = [
  { n: 1, title: 'Pick a category', body: 'Four tracks, from C# and EF Core to SQL and engineering practices.' },
  { n: 2, title: 'Answer & learn', body: 'Instant scoring and explanations after every quiz you complete.' },
  { n: 3, title: 'Track your climb', body: 'Watch streaks, accuracy and category strength grow on your dashboard.' },
]

export function LandingPage() {
  return (
    <div className="min-h-screen bg-base text-primary">
      <header className="mx-auto flex max-w-[1560px] items-center gap-8 px-6 py-6 lg:px-16">
        <Link to="/" className="flex items-center gap-2.5">
          <span className="flex h-9 w-9 items-center justify-center rounded-[11px] bg-brand font-display text-[19px] font-extrabold text-brandfg">
            T
          </span>
          <span className="font-display text-[20px] font-bold tracking-tight">TechQuiz</span>
        </Link>
        <nav className="ml-3 hidden items-center gap-7 md:flex">
          <a href="#features" className="text-[15px] font-medium text-secondary hover:text-primary">
            Features
          </a>
          <a href="#demo" className="text-[15px] font-medium text-secondary hover:text-primary">
            Demo
          </a>
          <a href="#how" className="text-[15px] font-medium text-secondary hover:text-primary">
            How it works
          </a>
        </nav>
        <div className="ml-auto flex items-center gap-4">
          <ThemeToggle />
          <Link to="/login" className="hidden text-[15px] font-medium hover:text-accent-text sm:block">
            Sign in
          </Link>
          <Link
            to="/login"
            className="rounded-pill bg-btn px-5 py-3 text-[15px] font-semibold text-white shadow-float hover:opacity-90"
          >
            Get started
          </Link>
        </div>
      </header>

      {/* Hero */}
      <section className="relative overflow-hidden">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-40 -top-80 h-[1000px] w-[1000px] rounded-full"
          style={{ background: 'radial-gradient(circle, var(--hero-glow-1), transparent 60%)' }}
        />
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -left-64 bottom-[-420px] h-[1000px] w-[1000px] rounded-full"
          style={{ background: 'radial-gradient(circle, var(--hero-glow-2), transparent 60%)' }}
        />
        <div className="relative mx-auto grid max-w-[1560px] items-center gap-12 px-6 py-16 lg:grid-cols-2 lg:gap-20 lg:px-16 lg:py-24">
          <div>
            <span className="mb-7 inline-flex items-center gap-2 rounded-pill bg-amber-bg px-4 py-2 text-[14px] font-semibold text-amber-text">
              <span aria-hidden="true">🔥</span> Keep your streak going
            </span>
            <h1 className="mb-6 font-display text-[clamp(44px,6vw,80px)] font-extrabold leading-[1.0] tracking-[-0.025em]">
              Level up your
              <br />
              .NET skills,
              <br />
              one quiz a day.
            </h1>
            <p className="mb-9 max-w-[520px] text-[clamp(17px,1.4vw,21px)] leading-[1.6] text-secondary">
              Build streaks and watch your progress climb across C#, ASP.NET&nbsp;Core, EF&nbsp;Core
              and SQL — with AI-generated questions and spaced repetition.
            </p>
            <div className="flex flex-wrap items-center gap-3.5">
              <Link
                to="/login"
                className="rounded-pill bg-btn px-8 py-4 text-[17px] font-semibold text-white shadow-float hover:opacity-90"
              >
                Get started free
              </Link>
              <Link
                to="/login"
                className="rounded-pill border border-strong bg-surface px-7 py-4 text-[17px] font-semibold hover:bg-elevated"
              >
                Try the demo
              </Link>
            </div>
            <div className="mt-11 flex gap-8">
              <Stat value="269" label="questions" />
              <Stat value="18" label="topics" />
              <Stat value="100%" label="free forever" />
            </div>
          </div>

          <HeroCard />
        </div>
      </section>

      {/* Topics */}
      <section className="mx-auto max-w-[1560px] px-6 pb-14 lg:px-16">
        <div className="flex flex-wrap items-center gap-2.5 border-t border-default pt-11">
          <span className="mr-2 text-[15px] font-medium text-secondary">Topics you can master</span>
          {TOPICS.map((topic, index) => (
            <span
              key={topic}
              className={`rounded-pill border px-4 py-2 font-mono text-[14px] font-medium ${
                index === 0
                  ? 'border-strong bg-elevated text-amber-text'
                  : 'border-default bg-elevated text-secondary'
              }`}
            >
              {topic}
            </span>
          ))}
          <span className="rounded-pill border border-default bg-elevated px-4 py-2 font-mono text-[14px] text-muted">
            +11 more
          </span>
        </div>
      </section>

      {/* Product walkthrough video. Deliberately not autoplaying: it runs well past five
          seconds, and WCAG 2.2.2 would then require a pause control. Click-to-play with
          preload="none" also keeps the landing page's first load cheap. */}
      <section id="demo" className="mx-auto max-w-[1560px] px-6 pb-20 lg:px-16">
        <h2 className="mb-2.5 max-w-[640px] font-display text-[clamp(28px,3vw,42px)] font-extrabold leading-[1.1] tracking-[-0.02em]">
          See it in action.
        </h2>
        <p className="mb-8 max-w-[560px] text-[clamp(15px,1.2vw,18px)] text-secondary">
          A 44-second walkthrough: signing in with the demo account, browsing tracks, taking a quiz,
          reviewing your score — and a look under the hood at the architecture.
        </p>
        <div className="mx-auto max-w-[1100px] overflow-hidden rounded-[22px] border border-default bg-surface shadow-float">
          <video
            className="block aspect-video w-full"
            controls
            preload="none"
            playsInline
            poster="/media/techquiz-poster.png"
          >
            <source src="/media/techquiz-walkthrough.mp4" type="video/mp4" />
            Your browser can’t play embedded video —{' '}
            <a href="/media/techquiz-walkthrough.mp4">download the walkthrough</a> instead.
          </video>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="mx-auto max-w-[1560px] px-6 pb-20 lg:px-16">
        <h2 className="mb-2.5 max-w-[640px] font-display text-[clamp(28px,3vw,42px)] font-extrabold leading-[1.1] tracking-[-0.02em]">
          Everything you need to actually retain what you learn.
        </h2>
        <p className="mb-10 max-w-[560px] text-[clamp(15px,1.2vw,18px)] text-secondary">
          Not another flashcard app — a feedback loop built for developers.
        </p>
        <div className="grid gap-5 [grid-template-columns:repeat(auto-fit,minmax(280px,1fr))]">
          {FEATURES.map((feature) => (
            <div key={feature.title} className="rounded-[22px] border border-default bg-surface p-8">
              <div
                aria-hidden="true"
                className="mb-5 flex h-[50px] w-[50px] items-center justify-center rounded-[14px] bg-amber-bg text-[24px]"
              >
                {feature.icon}
              </div>
              <h3 className="mb-2 font-display text-[21px] font-bold">{feature.title}</h3>
              <p className="text-[15px] leading-[1.6] text-secondary">{feature.body}</p>
            </div>
          ))}
        </div>
      </section>

      {/* How it works */}
      <section id="how" className="mx-auto max-w-[1560px] px-6 pb-24 lg:px-16">
        <div className="relative overflow-hidden rounded-[28px] border border-strong bg-card-grad p-8 sm:p-14">
          <div
            aria-hidden="true"
            className="pointer-events-none absolute -right-20 -top-28 h-[420px] w-[420px] rounded-full"
            style={{ background: 'radial-gradient(circle, var(--hero-glow-1), transparent 62%)' }}
          />
          <div className="relative grid items-center gap-11 [grid-template-columns:repeat(auto-fit,minmax(360px,1fr))]">
            <div>
              <h2 className="mb-4 font-display text-[clamp(30px,3.2vw,46px)] font-extrabold leading-[1.08] tracking-[-0.02em]">
                Ready when you are.
              </h2>
              <p className="mb-8 max-w-[440px] text-[clamp(16px,1.3vw,19px)] text-secondary">
                Sign in with the demo account and take your first quiz in under a minute — no setup,
                no card required.
              </p>
              <Link
                to="/login"
                className="inline-block rounded-pill bg-btn px-8 py-4 text-[17px] font-semibold text-white shadow-float hover:opacity-90"
              >
                Get started free
              </Link>
            </div>
            <ol className="flex list-none flex-col gap-4">
              {STEPS.map((step) => (
                <li key={step.n} className="flex items-start gap-4">
                  <span className="flex h-[34px] w-[34px] shrink-0 items-center justify-center rounded-pill bg-brand font-display text-[15px] font-extrabold text-brandfg">
                    {step.n}
                  </span>
                  <div>
                    <p className="font-display text-[17px] font-bold">{step.title}</p>
                    <p className="text-[14px] text-secondary">{step.body}</p>
                  </div>
                </li>
              ))}
            </ol>
          </div>
        </div>
      </section>

      <footer className="border-t border-default">
        <div className="mx-auto flex max-w-[1560px] flex-wrap items-center gap-4 px-6 py-8 lg:px-16">
          <div className="flex items-center gap-2.5">
            <span className="flex h-[26px] w-[26px] items-center justify-center rounded-lg bg-brand font-display text-[13px] font-extrabold text-brandfg">
              T
            </span>
            <span className="font-display text-[15px] font-bold">TechQuiz</span>
          </div>
          <span className="ml-auto font-mono text-[13px] text-muted">© 2026 TechQuiz · v1.0.0</span>
        </div>
      </footer>
    </div>
  )
}

function Stat({ value, label }: { value: string; label: string }) {
  return (
    <div>
      <div className="font-display text-[30px] font-extrabold">{value}</div>
      <div className="text-[14px] text-muted">{label}</div>
    </div>
  )
}

// Static marketing visual — a floating "daily goal" card. Not wired to any data.
function HeroCard() {
  return (
    <div className="auth-float-hero mx-auto w-full max-w-[480px] rounded-[26px] border border-default bg-surface p-8 shadow-float">
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-3.5">
          <div className="flex h-[52px] w-[52px] items-center justify-center rounded-[15px] bg-brand font-display text-[20px] font-extrabold text-brandfg">
            C#
          </div>
          <div>
            <p className="mb-0.5 text-[13px] font-semibold text-accent-text">Daily goal</p>
            <p className="font-display text-[19px] font-bold">C# Advanced</p>
          </div>
        </div>
        <span className="rounded-pill bg-amber-bg px-3 py-1.5 font-mono text-[13px] font-semibold text-amber-text">
          87%
        </span>
      </div>
      <div className="mb-3 h-3 overflow-hidden rounded-pill bg-track">
        <div className="h-full rounded-pill bg-brand" style={{ width: '87%' }} />
      </div>
      <div className="mb-6 flex justify-between text-[14px] text-secondary">
        <span className="font-medium">13 / 15 correct</span>
        <span>Today’s quiz</span>
      </div>
      <div className="flex gap-2.5">
        {[
          { value: '12', label: 'day streak' },
          { value: '269', label: 'questions' },
          { value: '84%', label: 'accuracy' },
        ].map((tile) => (
          <div key={tile.label} className="flex-1 rounded-[14px] bg-elevated p-3.5">
            <div className="font-display text-[22px] font-extrabold">{tile.value}</div>
            <div className="text-[12px] text-muted">{tile.label}</div>
          </div>
        ))}
      </div>
    </div>
  )
}
