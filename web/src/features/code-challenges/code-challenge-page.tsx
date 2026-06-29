import { useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import Editor from '@monaco-editor/react'
import { isAxiosError } from 'axios'
import { useCodeChallenges } from './use-code-challenges'
import { useRunCode } from './use-run-code'
import { useGradeChallenge } from './use-grade-challenge'
import { useCodeFeedback } from './use-code-feedback'
import { AI_PROVIDERS } from '../settings/api'
import { useConfiguredProviders } from '../settings/use-configured-providers'
import type { CodeChallengeGradeResult, CodeExecutionResult, CodeFeedbackResult } from './api'

const DIFFICULTY_BADGE: Record<string, { text: string; bg: string }> = {
  Easy: { text: 'text-success', bg: 'rgba(16,185,129,0.1)' },
  Medium: { text: 'text-warning', bg: 'rgba(245,158,11,0.1)' },
  Hard: { text: 'text-danger', bg: 'rgba(239,68,68,0.1)' },
}

export function CodeChallengePage() {
  const { id = '' } = useParams()
  // No single-challenge GET exists; the list is cheap static seed, so we reuse its cache and
  // pick the one we're on. Falls back to a fetch if the user deep-links straight here.
  const { data: challenges, isLoading, isError } = useCodeChallenges()
  const challenge = useMemo(
    () => challenges?.find((c) => c.id === id),
    [challenges, id],
  )

  if (isLoading) {
    return <Centered>Loading challenge…</Centered>
  }
  if (isError) {
    return <Centered tone="danger">Couldn’t load this challenge.</Centered>
  }
  if (!challenge) {
    return <Centered tone="danger">Challenge not found.</Centered>
  }

  return <ChallengeEditor key={challenge.id} challenge={challenge} />
}

function ChallengeEditor({
  challenge,
}: {
  challenge: NonNullable<ReturnType<typeof useCodeChallenges>['data']>[number]
}) {
  const [source, setSource] = useState(challenge.starterCode ?? '')
  const [stdin, setStdin] = useState('')
  // Which action produced the panel below, so Run output, the grade verdict, and AI feedback
  // don't fight for the same space — we show whichever the user triggered last.
  const [lastAction, setLastAction] = useState<'run' | 'grade' | 'feedback' | null>(null)

  const run = useRunCode()
  const grade = useGradeChallenge(challenge.id)
  const feedback = useCodeFeedback(challenge.id)
  const badge = DIFFICULTY_BADGE[challenge.difficulty]

  // Feedback needs the user's own key, so it's only offered for a provider that has a live client
  // AND a stored key. We default to the first such provider (Anthropic today); with none, the
  // button is disabled and we point the user at Settings.
  const { data: configured } = useConfiguredProviders()
  const feedbackProvider = AI_PROVIDERS.find(
    (provider) => provider.available && (configured?.includes(provider.id) ?? false),
  )

  function handleRun() {
    setLastAction('run')
    run.mutate({ sourceCode: source, stdin: stdin === '' ? null : stdin })
  }

  function handleSubmit() {
    setLastAction('grade')
    grade.mutate(source)
  }

  function handleFeedback() {
    if (!feedbackProvider) return
    setLastAction('feedback')
    feedback.mutate({ sourceCode: source, provider: feedbackProvider.id })
  }

  const busy = run.isPending || grade.isPending || feedback.isPending

  return (
    <main className="mx-auto max-w-4xl px-6 py-8 sm:px-9">
      <Link to="/challenges" className="mb-5 inline-block text-[12px] text-secondary hover:text-primary">
        ← All challenges
      </Link>

      <div className="mb-2 flex items-start justify-between gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">{challenge.title}</h1>
        {badge ? (
          <span
            className={`mt-1 shrink-0 rounded-full px-2 py-0.5 font-mono text-[10px] ${badge.text}`}
            style={{ backgroundColor: badge.bg }}
          >
            {challenge.difficulty}
          </span>
        ) : null}
      </div>
      <p className="mb-5 whitespace-pre-line text-[13px] leading-relaxed text-secondary">
        {challenge.prompt}
      </p>

      <div className="overflow-hidden rounded-[10px] border border-default">
        <Editor
          height="360px"
          defaultLanguage="csharp"
          theme="vs-dark"
          value={source}
          onChange={(value) => setSource(value ?? '')}
          options={{
            minimap: { enabled: false },
            fontSize: 13,
            scrollBeyondLastLine: false,
            tabSize: 4,
          }}
        />
      </div>

      <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-end">
        <label className="flex-1 text-[12px] text-secondary">
          <span className="mb-1 block">Custom input (stdin) for Run</span>
          <textarea
            value={stdin}
            onChange={(event) => setStdin(event.target.value)}
            rows={2}
            placeholder="e.g. 2 3"
            className="w-full resize-y rounded-[8px] border border-default bg-surface px-2.5 py-1.5 font-mono text-[12px] text-primary placeholder:text-muted focus:border-accent focus:outline-none"
          />
        </label>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={handleRun}
            disabled={busy}
            className="rounded-[8px] border border-default bg-surface px-3.5 py-2 text-[13px] font-medium text-primary hover:border-accent disabled:opacity-50"
          >
            {run.isPending ? 'Running…' : 'Run'}
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={busy}
            className="rounded-[8px] bg-accent px-3.5 py-2 text-[13px] font-medium text-white hover:opacity-90 disabled:opacity-50"
          >
            {grade.isPending ? 'Submitting…' : 'Submit'}
          </button>
          <button
            type="button"
            onClick={handleFeedback}
            disabled={busy || !feedbackProvider}
            title={feedbackProvider ? undefined : 'Add an AI provider key in Settings first'}
            className="rounded-[8px] border border-default bg-surface px-3.5 py-2 text-[13px] font-medium text-primary hover:border-accent disabled:opacity-50"
          >
            {feedback.isPending ? 'Asking AI…' : 'Get AI feedback'}
          </button>
        </div>
      </div>

      {!feedbackProvider ? (
        <p className="mt-2 text-[11px] text-muted">
          AI feedback needs your own provider key —{' '}
          <Link to="/settings" className="text-accent hover:underline">
            add one in Settings
          </Link>
          .
        </p>
      ) : null}

      <div className="mt-5">
        {lastAction === 'run' ? (
          run.isError ? (
            <Panel tone="danger">Run failed. Please try again.</Panel>
          ) : run.data ? (
            <RunOutput result={run.data} />
          ) : null
        ) : lastAction === 'grade' ? (
          grade.isError ? (
            <Panel tone="danger">Grading failed. Please try again.</Panel>
          ) : grade.data ? (
            <Verdict result={grade.data} />
          ) : null
        ) : lastAction === 'feedback' ? (
          feedback.isPending ? (
            <Panel>Asking the AI for feedback…</Panel>
          ) : feedback.isError ? (
            <Panel tone="danger">{feedbackErrorMessage(feedback.error)}</Panel>
          ) : feedback.data ? (
            <Feedback result={feedback.data} />
          ) : null
        ) : null}
      </div>
    </main>
  )
}

function feedbackErrorMessage(error: unknown): string {
  if (isAxiosError(error) && error.response?.status === 409) {
    return 'No key configured for that provider. Add one in Settings.'
  }
  return 'Couldn’t get feedback. Please try again.'
}

// AI feedback: qualitative prose, explicitly complementary to the test verdict (ADR-018) — never
// a score. Rendered as plain prose with preserved line breaks; rich markdown is deferred (no
// markdown dependency yet).
function Feedback({ result }: { result: CodeFeedbackResult }) {
  return (
    <div className="rounded-[10px] border border-default bg-surface p-3.5">
      <div className="mb-2 flex items-center gap-2 text-[12px]">
        <span className="font-semibold text-primary">AI feedback</span>
        <span className="rounded-full bg-elevated px-1.5 py-px font-mono text-[10px] text-secondary">
          {result.provider}
        </span>
      </div>
      <p className="whitespace-pre-line text-[13px] leading-relaxed text-secondary">
        {result.feedback}
      </p>
      <p className="mt-2.5 text-[10px] text-muted">
        Complementary guidance — the tests above decide pass or fail.
      </p>
    </div>
  )
}

// Ad-hoc Run: the raw sandbox verdict for the user's custom stdin.
function RunOutput({ result }: { result: CodeExecutionResult }) {
  return (
    <div className="rounded-[10px] border border-default bg-surface p-3.5">
      <div className="mb-2 flex items-center gap-2 text-[12px]">
        <span className="font-semibold text-primary">Run</span>
        <span className="rounded-full bg-elevated px-1.5 py-px font-mono text-[10px] text-secondary">
          {result.status}
        </span>
      </div>
      {result.compileOutput ? (
        <OutputBlock label="Compiler output" body={result.compileOutput} tone="danger" />
      ) : null}
      {result.stdout ? <OutputBlock label="stdout" body={result.stdout} /> : null}
      {result.stderr ? <OutputBlock label="stderr" body={result.stderr} tone="danger" /> : null}
    </div>
  )
}

// Grade verdict: two-stage, mirroring the backend compile gate. When it didn't compile we show
// the compiler output and no test rows; when it compiled we show the per-case pass/fail summary.
function Verdict({ result }: { result: CodeChallengeGradeResult }) {
  if (!result.compiled) {
    return (
      <div className="rounded-[10px] border border-danger bg-surface p-3.5">
        <p className="mb-2 text-[13px] font-semibold text-danger">Doesn’t compile</p>
        <OutputBlock label="Compiler output" body={result.compileOutput ?? '(no output)'} tone="danger" />
      </div>
    )
  }

  const allPassed = result.passed
  return (
    <div className="rounded-[10px] border border-default bg-surface p-3.5">
      <p
        className={`mb-3 text-[13px] font-semibold ${allPassed ? 'text-success' : 'text-warning'}`}
      >
        {allPassed ? 'All tests passed' : 'Some tests failed'} — {result.passedCount}/
        {result.totalCount}
      </p>
      <ul className="flex flex-col gap-1.5">
        {result.cases.map((testCase) => (
          <li
            key={testCase.orderIndex}
            className="flex items-center gap-2 text-[12px] text-secondary"
          >
            <span className={testCase.passed ? 'text-success' : 'text-danger'}>
              {testCase.passed ? '✓' : '✗'}
            </span>
            <span>Test {testCase.orderIndex + 1}</span>
            {!testCase.passed ? (
              <span className="rounded-full bg-elevated px-1.5 py-px font-mono text-[10px] text-muted">
                {testCase.status}
              </span>
            ) : null}
          </li>
        ))}
      </ul>
    </div>
  )
}

function OutputBlock({
  label,
  body,
  tone,
}: {
  label: string
  body: string
  tone?: 'danger'
}) {
  return (
    <div className="mb-2 last:mb-0">
      <p className="mb-1 text-[10px] uppercase tracking-wide text-muted">{label}</p>
      <pre
        className={`overflow-x-auto rounded-[6px] bg-[#0d1117] p-2.5 font-mono text-[11px] leading-relaxed ${
          tone === 'danger' ? 'text-danger' : 'text-secondary'
        }`}
      >
        {body}
      </pre>
    </div>
  )
}

function Panel({ children, tone }: { children: React.ReactNode; tone?: 'danger' }) {
  return (
    <div
      className={`rounded-[10px] border bg-surface p-3.5 text-[13px] ${
        tone === 'danger' ? 'border-danger text-danger' : 'border-default text-secondary'
      }`}
    >
      {children}
    </div>
  )
}

function Centered({ children, tone }: { children: React.ReactNode; tone?: 'danger' }) {
  return (
    <main className="mx-auto max-w-4xl px-6 py-8 sm:px-9">
      <p className={`text-sm ${tone === 'danger' ? 'text-danger' : 'text-secondary'}`}>{children}</p>
    </main>
  )
}
