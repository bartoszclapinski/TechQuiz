import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { isAxiosError } from 'axios'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useAuth } from './use-auth'
import { AuthLayout } from './auth-layout'

const registerSchema = z
  .object({
    email: z.email('Enter a valid email address'),
    password: z.string().min(8, 'Use at least 8 characters'),
    confirmPassword: z.string().min(1, 'Confirm your password'),
  })
  .refine((values) => values.password === values.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Passwords do not match',
  })

type RegisterValues = z.infer<typeof registerSchema>

type RegisterField = 'email' | 'password'

// The API returns RFC 7807 ProblemDetails with an `errors` extension whose shape depends on the
// failure: a flat string[] for Identity policy failures (RegistrationFailedException — "email
// taken", "needs a digit") or a { field: string[] } dict for FluentValidation. Normalise both to
// per-field messages, routing anything mentioning "password" to the password field and the rest
// to email, so the user sees the real reason on the right control instead of a fixed guess.
function fieldErrorsFromProblem(data: unknown): Record<RegisterField, string | undefined> {
  const raw = (data as { errors?: unknown } | null)?.errors
  const entries: { key?: string; message: string }[] = []

  if (Array.isArray(raw)) {
    for (const message of raw) {
      if (typeof message === 'string') entries.push({ message })
    }
  } else if (raw && typeof raw === 'object') {
    for (const [key, value] of Object.entries(raw as Record<string, unknown>)) {
      if (Array.isArray(value)) {
        for (const message of value) {
          if (typeof message === 'string') entries.push({ key, message })
        }
      }
    }
  }

  const byField: Record<RegisterField, string[]> = { email: [], password: [] }
  for (const entry of entries) {
    const field: RegisterField = /password/i.test(entry.key ?? entry.message) ? 'password' : 'email'
    byField[field].push(entry.message)
  }

  return {
    email: byField.email.length > 0 ? byField.email.join(' ') : undefined,
    password: byField.password.length > 0 ? byField.password.join(' ') : undefined,
  }
}

export function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', password: '', confirmPassword: '' },
  })

  async function onSubmit(values: RegisterValues) {
    try {
      await registerUser(values.email, values.password)
      navigate('/dashboard', { replace: true })
    } catch (error) {
      // 400 (validation / Identity policy) and 409 (conflict) mean the API rejected the input —
      // surface the server's own messages on the matching fields. Anything else is unexpected.
      const status = isAxiosError(error) ? error.response?.status : undefined
      if (status === 400 || status === 409) {
        const fieldErrors = fieldErrorsFromProblem(
          isAxiosError(error) ? error.response?.data : undefined,
        )
        if (fieldErrors.email) setError('email', { message: fieldErrors.email })
        if (fieldErrors.password) setError('password', { message: fieldErrors.password })
        if (!fieldErrors.email && !fieldErrors.password) {
          setError('email', {
            message: 'That email may already be registered, or the password is too weak.',
          })
        }
        return
      }
      toast.error('Something went wrong. Please try again.')
    }
  }

  return (
    <AuthLayout>
      <div className="mb-8">
        <h1 className="mb-2 font-display text-[clamp(32px,3vw,42px)] font-extrabold leading-[1.08] tracking-[-0.02em]">
          Create your account.
        </h1>
        <p className="text-[16px] text-secondary">
          Start tracking your progress across technical categories.
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

        <div className="mb-3.5">
          <label htmlFor="password" className="mb-1.5 block text-[13px] font-medium text-secondary">
            Password
          </label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
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

        <div className="mb-5">
          <label
            htmlFor="confirmPassword"
            className="mb-1.5 block text-[13px] font-medium text-secondary"
          >
            Confirm password
          </label>
          <input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            placeholder="••••••••••"
            aria-invalid={errors.confirmPassword ? true : undefined}
            aria-describedby={errors.confirmPassword ? 'confirmPassword-error' : undefined}
            {...register('confirmPassword')}
            className="w-full rounded-[14px] border border-default bg-elevated px-4 py-3 text-[16px] outline-none transition-shadow focus:border-accent focus:shadow-focus"
          />
          {errors.confirmPassword ? (
            <p id="confirmPassword-error" className="mt-1.5 text-[13px] text-danger">
              {errors.confirmPassword.message}
            </p>
          ) : null}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-[14px] bg-btn px-4 py-3.5 text-[16px] font-semibold text-white shadow-float transition-opacity hover:opacity-90 disabled:opacity-60"
        >
          {isSubmitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>

      <p className="mt-7 text-center text-[14px] text-secondary">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-accent-text hover:underline">
          Sign in
        </Link>
      </p>
    </AuthLayout>
  )
}
