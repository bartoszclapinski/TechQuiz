import { useParams } from 'react-router-dom'

export function ResultPage() {
  const { attemptId } = useParams<{ attemptId: string }>()
  return (
    <section className="mx-auto max-w-2xl px-6 py-12">
      <h1 className="text-2xl font-semibold">Result</h1>
      <p className="mt-2 text-secondary">
        Score breakdown for attempt <span className="font-mono text-accent-text">{attemptId}</span>{' '}
        lands in iteration 1.7.
      </p>
    </section>
  )
}
