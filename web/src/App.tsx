import { ThemeToggle } from './components/ui/theme-toggle'

function App() {
  return (
    <main className="min-h-screen bg-base text-primary">
      <header className="flex items-center justify-between border-b px-6 py-4">
        <span className="font-semibold">TechQuiz</span>
        <ThemeToggle />
      </header>
      <section className="mx-auto max-w-2xl px-6 py-16">
        <h1 className="text-3xl font-semibold">Design system check</h1>
        <p className="mt-3 text-secondary">
          Toggle the theme in the top-right. Colors come from semantic tokens, so the
          whole page flips on a single <code className="font-mono text-accent-text">data-theme</code> attribute.
        </p>
        <div className="mt-8 rounded-xl border bg-surface p-6">
          <p className="text-sm text-muted">Surface card</p>
          <p className="mt-2">
            Body text on <span className="text-accent">accent</span>, with a{' '}
            <span className="rounded-full bg-accent-bg px-2 py-0.5 text-xs text-accent-text">
              pill
            </span>{' '}
            sample.
          </p>
        </div>
      </section>
    </main>
  )
}

export default App
