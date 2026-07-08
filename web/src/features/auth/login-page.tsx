import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { isAxiosError } from 'axios'
import { useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useAuth } from './use-auth'
import { AuthLayout } from './auth-layout'

const DEMO_EMAIL = 'demo@techquiz.local'
const DEMO_PASSWORD = 'DemoPass123!'

const loginSchema = z.object({
  email: z.email('Enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
})

type LoginValues = z.infer<typeof loginSchema>

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [demoLoading, setDemoLoading] = useState(false)

  const redirectTo = (location.state as { from?: string } | null)?.from ?? '/dashboard'

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  // 401 means the credentials were wrong — surface that inline on the form. Anything else
  // (network, 500) isn't the user's fault, so it goes to a toast instead of the field.
  function reportSignInError(error: unknown) {
    if (isAxiosError(error) && error.response?.status === 401) {
      setError('password', { message: 'Incorrect email or password' })
      return
    }
    toast.error('Something went wrong. Please try again.')
  }

  async function onSubmit(values: LoginValues) {
    try {
      await login(values.email, values.password)
      navigate(redirectTo, { replace: true })
    } catch (error) {
      reportSignInError(error)
    }
  }

  async function onDemo() {
    setDemoLoading(true)
    try {
      await login(DEMO_EMAIL, DEMO_PASSWORD)
      navigate(redirectTo, { replace: true })
    } catch (error) {
      reportSignInError(error)
    } finally {
      setDemoLoading(false)
    }
  }

  const busy = isSubmitting || demoLoading

  return (
    <AuthLayout>
      <div className="mb-8">
        <h1 className="mb-2 font-display text-[clamp(32px,3vw,42px)] font-extrabold leading-[1.08] tracking-[-0.02em]">
          Welcome back! 👋
        </h1>
        <p className="text-[16px] text-secondary">
          Continue where you left off, or start fresh with a new category.
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <div className="mb-3.5">
          <label htmlFor="email" className="mb-1.5 block text-[13px] font-medium text-secondary">
            Email
          </label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@example.com"
            aria-invalid={errors.email ? true : undefined}
            aria-describedby={errors.email ? 'email-error' : undefined}
            {...register('email')}
            className="w-full rounded-[14px] border border-default bg-elevated px-4 py-3 text-[16px] outline-none transition-shadow focus:border-accent focus:shadow-focus"
          />
          {errors.email ? (
            <p id="email-error" className="mt-1.5 text-[13px] text-danger">
              {errors.email.message}
            </p>
          ) : null}
        </div>

        <div className="mb-5">
          <div className="mb-1.5 flex items-center justify-between">
            <label htmlFor="password" className="text-[13px] font-medium text-secondary">
              Password
            </label>
            <button
              type="button"
              onClick={() => toast.info('Password recovery arrives in a later phase.')}
              className="text-[13px] text-secondary transition-colors hover:text-primary"
            >
              Forgot password?
            </button>
          </div>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            placeholder="••••••••••"
            aria-invalid={errors.password ? true : undefined}
            aria-describedby={errors.password ? 'password-error' : undefined}
            {...register('password')}
            className="w-full rounded-[14px] border border-default bg-elevated px-4 py-3 text-[16px] outline-none transition-shadow focus:border-accent focus:shadow-focus"
          />
          {errors.password ? (
            <p id="password-error" className="mt-1.5 text-[13px] text-danger">
              {errors.password.message}
            </p>
          ) : null}
        </div>

        <button
          type="submit"
          disabled={busy}
          className="w-full rounded-[14px] bg-btn px-4 py-3.5 text-[16px] font-semibold text-white shadow-float transition-opacity hover:opacity-90 disabled:opacity-60"
        >
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>

      <div className="my-4 flex items-center gap-3">
        <div className="h-px flex-1 bg-[var(--border-default)]" />
        <span className="font-mono text-[13px] uppercase tracking-[0.08em] text-muted">or</span>
        <div className="h-px flex-1 bg-[var(--border-default)]" />
      </div>

      <button
        type="button"
        onClick={onDemo}
        disabled={busy}
        className="flex w-full items-center justify-center gap-2 rounded-[14px] border border-strong bg-transparent px-4 py-3.5 text-[16px] font-semibold transition-colors hover:bg-elevated disabled:opacity-60"
      >
        <svg
          width="14"
          height="14"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
        >
          <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4" />
          <polyline points="10 17 15 12 10 7" />
          <line x1="15" y1="12" x2="3" y2="12" />
        </svg>
        {demoLoading ? 'Signing in…' : 'Continue as demo'}
      </button>

      <p className="mt-7 text-center text-[14px] text-secondary">
        Public sign-ups are paused during the demo — use{' '}
        <span className="font-medium text-primary">Continue as demo</span> above to explore.
      </p>
    </AuthLayout>
  )
}
