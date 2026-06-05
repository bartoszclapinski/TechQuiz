import { useParams } from 'react-router-dom'

export function QuizPage() {
  const { id } = useParams<{ id: string }>()
  return (
    <section className="mx-auto max-w-2xl px-6 py-12">
      <h1 className="text-2xl font-semibold">Quiz</h1>
      <p className="mt-2 text-secondary">
        Quiz runner for attempt <span className="font-mono text-accent-text">{id}</span> lands in
        iteration 1.6. The topbar is intentionally hidden on this route for a focused experience.
      </p>
    </section>
  )
}
