import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { isAxiosError } from 'axios'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'
import { AI_PROVIDERS } from '../settings/api'
import { useConfiguredProviders } from '../settings/use-configured-providers'
import { usePublishQuestion } from '../pool/use-publish-question'
import { useGenerateQuestions } from './use-generate-questions'
import type { GenerateResult } from './api'

const DIFFICULTIES = ['Easy', 'Medium', 'Hard'] as const

// Difficulty arrives as a name here (the AI contract is string-typed), unlike the numeric quiz
// feature. Badge colors follow ADR-015 (emerald/amber/red at ~10% opacity).
const DIFFICULTY_BADGE: Record<string, { text: string; bg: string }> = {
  Easy: { text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  Medium: { text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  Hard: { text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
}

const generateSchema = z.object({
  topic: z.string().trim().min(1, 'Enter a topic'),
  difficulty: z.enum(DIFFICULTIES),
  // register('count', { valueAsNumber: true }) hands RHF a number (NaN when empty), so the schema
  // stays a plain number — keeping zod's input and output types aligned for the resolver.
  count: z
    .number({ error: 'Enter a number' })
    .int()
    .min(1, 'At least 1')
    .max(20, 'At most 20'),
  provider: z.string().min(1, 'Pick a provider'),
})

type GenerateValues = z.infer<typeof generateSchema>

const inputClass =
  'w-full rounded-lg border border-default bg-surface px-3.5 py-2.5 text-[15px] outline-none transition-shadow focus:border-accent focus:ring-1 focus:ring-accent'

export function GeneratePage() {
  const { data: configured, isLoading: providersLoading } = useConfiguredProviders()
  const generate = useGenerateQuestions()
  const [result, setResult] = useState<GenerateResult | null>(null)

  // A provider is usable only if it has a live client AND the user has stored a key for it.
  const availableProviders = AI_PROVIDERS.filter(
    (provider) => provider.available && (configured?.includes(provider.id) ?? false),
  )
  const hasProvider = availableProviders.length > 0

  const {
    register,
    handleSubmit,
    setValue,
    getValues,
    formState: { errors },
  } = useForm<GenerateValues>({
    resolver: zodResolver(generateSchema),
    defaultValues: { topic: '', difficulty: 'Easy', count: 5, provider: '' },
  })

  // The provider list loads async; seed the select with the first usable provider once it's known.
  useEffect(() => {
    if (hasProvider && !getValues('provider')) {
      setValue('provider', availableProviders[0].id)
    }
  }, [hasProvider, availableProviders, getValues, setValue])

  function onSubmit(values: GenerateValues) {
    generate.mutate(values, {
      onSuccess: (data) => setResult(data),
      onError: reportGenerateError,
    })
  }

  return (
    <main className="mx-auto max-w-3xl px-6 py-8 sm:px-9">
      <div className="mb-7">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Generate questions</h1>
        <p className="text-[14px] text-secondary">
          Draft new questions with your own AI provider. Review them below and publish the good ones
          to the shared pool.
        </p>
      </div>

      {providersLoading ? (
        <p className="text-[15px] text-secondary">Loading providers…</p>
      ) : !hasProvider ? (
        <div className="rounded-[10px] border border-default bg-surface p-4">
          <p className="mb-1 text-[14px] font-semibold text-primary">No provider configured</p>
          <p className="mb-3 text-[14px] text-secondary">
            Add an AI provider key before generating questions.
          </p>
          <Link
            to="/settings"
            className="inline-block rounded-lg bg-accent px-4 py-2 text-[15px] font-medium text-white transition-opacity hover:opacity-90"
          >
            Go to Settings
          </Link>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="rounded-[10px] border border-default bg-surface p-4">
          <div className="mb-3.5">
            <label htmlFor="topic" className="mb-1.5 block text-[13px] font-medium text-secondary">
              Topic
            </label>
            <input
              id="topic"
              type="text"
              placeholder="e.g. C# records, EF Core change tracking"
              aria-invalid={errors.topic ? true : undefined}
              {...register('topic')}
              className={inputClass}
            />
            {errors.topic ? <p className="mt-1.5 text-[13px] text-danger">{errors.topic.message}</p> : null}
          </div>

          <div className="mb-3.5 grid grid-cols-1 gap-3.5 sm:grid-cols-3">
            <div>
              <label htmlFor="difficulty" className="mb-1.5 block text-[13px] font-medium text-secondary">
                Difficulty
              </label>
              <select id="difficulty" {...register('difficulty')} className={inputClass}>
                {DIFFICULTIES.map((level) => (
                  <option key={level} value={level}>
                    {level}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="count" className="mb-1.5 block text-[13px] font-medium text-secondary">
                Count
              </label>
              <input
                id="count"
                type="number"
                min={1}
                max={20}
                aria-invalid={errors.count ? true : undefined}
                {...register('count', { valueAsNumber: true })}
                className={inputClass}
              />
              {errors.count ? <p className="mt-1.5 text-[13px] text-danger">{errors.count.message}</p> : null}
            </div>

            <div>
              <label htmlFor="provider" className="mb-1.5 block text-[13px] font-medium text-secondary">
                Provider
              </label>
              <select id="provider" {...register('provider')} className={inputClass}>
                {availableProviders.map((provider) => (
                  <option key={provider.id} value={provider.id}>
                    {provider.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <button
            type="submit"
            disabled={generate.isPending}
            aria-busy={generate.isPending}
            className="rounded-lg bg-accent px-4 py-2.5 text-[15px] font-medium text-white transition-opacity hover:opacity-90 disabled:opacity-60"
          >
            {generate.isPending ? 'Generating…' : 'Generate'}
          </button>
        </form>
      )}

      {result ? <DraftPreview result={result} /> : null}
    </main>
  )
}

function DraftPreview({ result }: { result: GenerateResult }) {
  // Publishing is per-draft: each draft is its own persisted question with its own id. We track
  // which ids have been published so the button flips to "Published" and can't re-fire (a second
  // publish would 409). The publish hook invalidates the pool query, so the new question shows up
  // on the Pool screen automatically.
  const publish = usePublishQuestion()
  const [publishedIds, setPublishedIds] = useState<Set<string>>(new Set())

  if (result.questions.length === 0) {
    return (
      <p className="mt-6 text-[15px] text-secondary">
        No questions came back. Try a different topic or a smaller count.
      </p>
    )
  }

  function onPublish(id: string) {
    publish.mutate(id, {
      onSuccess: () => {
        setPublishedIds((current) => new Set(current).add(id))
        toast.success('Published to the pool.')
      },
      onError: () => toast.error('Couldn’t publish that question. Please try again.'),
    })
  }

  return (
    <section className="mt-7">
      <h2 className="mb-3 text-[15px] font-semibold text-primary">
        {result.questions.length} draft{result.questions.length === 1 ? '' : 's'} from{' '}
        {result.provider}
      </h2>

      <div className="flex flex-col gap-2.5">
        {result.questions.map((draft) => {
          const badge = DIFFICULTY_BADGE[draft.difficulty]
          const isPublished = publishedIds.has(draft.id)
          const isPublishing = publish.isPending && publish.variables === draft.id
          return (
            <article key={draft.id} className="rounded-[10px] border border-default bg-base p-3.5">
              <div className="mb-2 flex items-start justify-between gap-3">
                <p className="text-[14px] font-semibold text-primary">{draft.stem}</p>
                {badge ? (
                  <span
                    className={`shrink-0 rounded-full px-2 py-0.5 font-mono text-[12px] ${badge.text}`}
                    style={{ backgroundColor: badge.bg }}
                  >
                    {draft.difficulty}
                  </span>
                ) : null}
              </div>
              <ul className="mb-2 flex flex-col gap-1">
                {draft.options.map((option, optionIndex) => (
                  <li key={optionIndex} className="text-[13px] text-secondary">
                    <span className="mr-1.5 font-mono text-[12px] text-muted">
                      {String.fromCharCode(65 + optionIndex)}.
                    </span>
                    {option}
                  </li>
                ))}
              </ul>
              {draft.explanation ? (
                <p className="mb-2 text-[13px] leading-snug text-muted">{draft.explanation}</p>
              ) : null}
              <div className="flex justify-end">
                {isPublished ? (
                  <span className="rounded-md px-2.5 py-1.5 text-[13px] font-medium text-success">
                    Published ✓
                  </span>
                ) : (
                  <button
                    type="button"
                    onClick={() => onPublish(draft.id)}
                    disabled={isPublishing}
                    aria-busy={isPublishing}
                    className="rounded-md bg-accent px-2.5 py-1.5 text-[13px] font-medium text-white transition-opacity hover:opacity-90 disabled:opacity-60"
                  >
                    {isPublishing ? 'Publishing…' : 'Publish to pool'}
                  </button>
                )}
              </div>
            </article>
          )
        })}
      </div>
    </section>
  )
}

function reportGenerateError(error: unknown) {
  if (isAxiosError(error) && error.response?.status === 409) {
    toast.error('No key configured for that provider. Add one in Settings.')
    return
  }
  if (isAxiosError(error) && error.response?.status === 400) {
    toast.error('That request was rejected. Check your inputs and try again.')
    return
  }
  toast.error('Generation failed. Please try again.')
}
