export function FullPageSpinner() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-base">
      <div
        role="status"
        aria-label="Loading"
        className="h-8 w-8 animate-spin rounded-full border-2 border-default border-t-accent"
      />
    </div>
  )
}
