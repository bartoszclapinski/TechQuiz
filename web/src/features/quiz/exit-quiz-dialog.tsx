import * as Dialog from '@radix-ui/react-dialog'

// Confirmation gate for leaving a quiz mid-attempt — exiting forfeits the attempt (ADR-015).
// Controlled by the runner so the same dialog answers both the header X and the Esc key.
export function ExitQuizDialog({
  open,
  onOpenChange,
  onConfirm,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: () => void
}) {
  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/60" />
        <Dialog.Content className="fixed left-1/2 top-1/2 z-50 w-[calc(100%-2rem)] max-w-sm -translate-x-1/2 -translate-y-1/2 rounded-xl border border-default bg-surface p-5 shadow-xl">
          <Dialog.Title className="text-[15px] font-semibold text-primary">Exit quiz?</Dialog.Title>
          <Dialog.Description className="mt-1.5 text-[13px] leading-snug text-secondary">
            Are you sure you want to exit? Your progress will be lost.
          </Dialog.Description>
          <div className="mt-5 flex justify-end gap-2.5">
            <Dialog.Close asChild>
              <button
                type="button"
                className="rounded-md border border-strong px-3.5 py-2 text-[13px] font-medium transition-colors hover:bg-elevated"
              >
                Cancel
              </button>
            </Dialog.Close>
            <button
              type="button"
              onClick={onConfirm}
              className="rounded-md bg-danger px-3.5 py-2 text-[13px] font-medium text-white transition-opacity hover:opacity-90"
            >
              Yes, exit
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
