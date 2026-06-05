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
      navigate('/categories', { replace: true })
    } catch (error) {
      // A 4xx means the API rejected the input (email taken or password too weak) — surface it
      // on the form. Anything else is unexpected and goes to a toast.
      if (isAxiosError(error) && error.response && error.response.status < 500) {
        setError('email', {
          message: 'That email may already be registered, or the password is too weak.',
        })
        return
      }
      toast.error('Something went wrong. Please try again.')
    }
  }

  return (
    <AuthLayout>
      <div className="mb-8">
        <h1 className="mb-2 text-[28px] font-semibold leading-tight tracking-tight">
          Create your account.
        </h1>
        <p className="text-sm text-secondary">
          Start tracking your progress across technical categories.
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <div className="mb-3.5">
          <label htmlFor="email" className="mb-1.5 block text-xs font-medium text-secondary">
            Email
          </label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@example.com"
            {...register('email')}
            className="w-full rounded-lg border border-default bg-surface px-3.5 py-2.5 text-sm outline-none transition-shadow focus:border-accent focus:ring-1 focus:ring-accent"
          />
          {errors.email ? (
            <p className="mt-1.5 text-xs text-danger">{errors.email.message}</p>
          ) : null}
        </div>

        <div className="mb-3.5">
          <label htmlFor="password" className="mb-1.5 block text-xs font-medium text-secondary">
            Password
          </label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            placeholder="••••••••••"
            {...register('password')}
            className="w-full rounded-lg border border-default bg-surface px-3.5 py-2.5 text-sm outline-none transition-shadow focus:border-accent focus:ring-1 focus:ring-accent"
          />
          {errors.password ? (
            <p className="mt-1.5 text-xs text-danger">{errors.password.message}</p>
          ) : null}
        </div>

        <div className="mb-5">
          <label
            htmlFor="confirmPassword"
            className="mb-1.5 block text-xs font-medium text-secondary"
          >
            Confirm password
          </label>
          <input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            placeholder="••••••••••"
            {...register('confirmPassword')}
            className="w-full rounded-lg border border-default bg-surface px-3.5 py-2.5 text-sm outline-none transition-shadow focus:border-accent focus:ring-1 focus:ring-accent"
          />
          {errors.confirmPassword ? (
            <p className="mt-1.5 text-xs text-danger">{errors.confirmPassword.message}</p>
          ) : null}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-lg bg-accent px-4 py-2.5 text-sm font-medium text-white transition-opacity hover:opacity-90 disabled:opacity-60"
        >
          {isSubmitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>

      <p className="mt-7 text-center text-[13px] text-secondary">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-accent-text hover:underline">
          Sign in
        </Link>
      </p>
    </AuthLayout>
  )
}
