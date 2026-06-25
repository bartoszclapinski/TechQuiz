import { useState, type FormEvent } from 'react'
import { isAxiosError } from 'axios'
import { toast } from 'sonner'
import { AI_PROVIDERS, type AiProvider } from './api'
import { useConfiguredProviders } from './use-configured-providers'
import { useSetAiKey } from './use-set-ai-key'
import { useRemoveAiKey } from './use-remove-ai-key'

export function SettingsPage() {
  const { data: configured, isLoading, isError, refetch } = useConfiguredProviders()

  return (
    <main className="mx-auto max-w-3xl px-6 py-8 sm:px-9">
      <div className="mb-7">
        <h1 className="mb-1 text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-[13px] text-secondary">
          Bring your own AI provider key to generate questions. Keys are encrypted at rest and are
          never shown again after you save them.
        </p>
      </div>

      <section>
        <h2 className="mb-3 text-sm font-semibold text-primary">AI providers</h2>

        {isLoading ? (
          <p className="text-sm text-secondary">Loading providers…</p>
        ) : isError ? (
          <div className="text-sm text-secondary">
            <p className="mb-2 text-danger">Could not load your providers.</p>
            <button
              type="button"
              onClick={() => void refetch()}
              className="rounded-md border border-strong px-3 py-1.5 text-sm font-medium transition-colors hover:bg-elevated"
            >
              Retry
            </button>
          </div>
        ) : (
          <div className="flex flex-col gap-2.5">
            {AI_PROVIDERS.map((provider) => (
              <ProviderRow
                key={provider.id}
                provider={provider}
                configured={configured?.includes(provider.id) ?? false}
              />
            ))}
          </div>
        )}
      </section>
    </main>
  )
}

function ProviderRow({ provider, configured }: { provider: AiProvider; configured: boolean }) {
  const setKey = useSetAiKey()
  const removeKey = useRemoveAiKey()
  const [keyInput, setKeyInput] = useState('')
  const [inlineError, setInlineError] = useState<string | null>(null)
  const [confirmingRemove, setConfirmingRemove] = useState(false)

  // Providers without a live client can't hold a usable key — render them as a disabled "soon"
  // row so the roadmap is visible without offering a dead-end action (ADR-014).
  if (!provider.available) {
    return (
      <div className="flex items-center justify-between rounded-[10px] border border-default bg-base p-3.5 opacity-60">
        <div>
          <p className="text-[13px] font-semibold text-primary">{provider.label}</p>
          <p className="text-[11px] text-muted">Not yet available</p>
        </div>
        <span className="rounded-full bg-elevated px-1.5 py-0.5 font-mono text-[9px] text-secondary">
          soon
        </span>
      </div>
    )
  }

  function onSave(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmed = keyInput.trim()
    if (!trimmed) {
      setInlineError('Enter a key.')
      return
    }
    setInlineError(null)
    setKey.mutate(
      { provider: provider.id, apiKey: trimmed },
      {
        onSuccess: () => {
          // Drop the key from component state the moment it's saved — the API never returns it,
          // and we never want it lingering in memory or the input.
          setKeyInput('')
          toast.success(`${provider.label} key saved.`)
        },
        onError: (error) => setInlineError(messageFromSetError(error)),
      },
    )
  }

  function onRemove() {
    removeKey.mutate(provider.id, {
      onSuccess: () => toast.success(`${provider.label} key removed.`),
      onSettled: () => setConfirmingRemove(false),
    })
  }

  return (
    <div className="rounded-[10px] border border-default bg-surface p-3.5">
      <div className="mb-3 flex items-center justify-between">
        <p className="text-[13px] font-semibold text-primary">{provider.label}</p>
        <span
          className={`rounded-full px-1.5 py-0.5 font-mono text-[10px] ${
            configured ? 'bg-accent-bg text-accent-text' : 'bg-elevated text-muted'
          }`}
        >
          {configured ? 'Configured' : 'Not configured'}
        </span>
      </div>

      <form onSubmit={onSave} className="flex items-start gap-2">
        <div className="flex-1">
          <input
            type="password"
            autoComplete="off"
            placeholder={configured ? 'Enter a new key to rotate' : 'Paste your API key'}
            value={keyInput}
            onChange={(event) => {
              setKeyInput(event.target.value)
              if (inlineError) setInlineError(null)
            }}
            aria-invalid={inlineError ? true : undefined}
            aria-describedby={inlineError ? `${provider.id}-error` : undefined}
            className="w-full rounded-lg border border-default bg-surface px-3.5 py-2 text-sm outline-none transition-shadow focus:border-accent focus:ring-1 focus:ring-accent"
          />
          {inlineError ? (
            <p id={`${provider.id}-error`} className="mt-1.5 text-xs text-danger">
              {inlineError}
            </p>
          ) : null}
        </div>
        <button
          type="submit"
          disabled={setKey.isPending}
          aria-busy={setKey.isPending}
          className="rounded-lg bg-accent px-4 py-2 text-sm font-medium text-white transition-opacity hover:opacity-90 disabled:opacity-60"
        >
          {setKey.isPending ? 'Saving…' : configured ? 'Rotate' : 'Save'}
        </button>
      </form>

      {configured ? (
        <div className="mt-3 border-t border-default pt-3">
          {confirmingRemove ? (
            <div className="flex items-center gap-2">
              <span className="text-xs text-secondary">Remove this key?</span>
              <button
                type="button"
                onClick={onRemove}
                disabled={removeKey.isPending}
                aria-busy={removeKey.isPending}
                className="rounded-md border border-strong px-2.5 py-1 text-xs font-medium text-danger transition-colors hover:bg-elevated disabled:opacity-60"
              >
                {removeKey.isPending ? 'Removing…' : 'Confirm remove'}
              </button>
              <button
                type="button"
                onClick={() => setConfirmingRemove(false)}
                disabled={removeKey.isPending}
                className="rounded-md px-2.5 py-1 text-xs font-medium text-secondary transition-colors hover:text-primary"
              >
                Cancel
              </button>
            </div>
          ) : (
            <button
              type="button"
              onClick={() => setConfirmingRemove(true)}
              className="text-xs font-medium text-secondary transition-colors hover:text-danger"
            >
              Remove key
            </button>
          )}
        </div>
      ) : null}
    </div>
  )
}

// A 400 is the API rejecting the key (empty/unknown provider) — actionable, so it goes inline.
// Anything else isn't the user's fault and gets a generic message.
function messageFromSetError(error: unknown): string {
  if (isAxiosError(error) && error.response?.status === 400) {
    return 'That key was rejected. Check it and try again.'
  }
  return 'Something went wrong. Please try again.'
}
