import { useMutation } from '@tanstack/react-query'
import { runCode } from './api'

// Running the editor's code against custom stdin is a write to the sandbox (it executes
// something), so it's a mutation, not a query — it's triggered on demand and has no cached
// result to keep fresh. The page reads `data`/`isPending` to render the run output.
export function useRunCode() {
  return useMutation({
    mutationFn: (vars: { sourceCode: string; stdin: string | null }) =>
      runCode(vars.sourceCode, vars.stdin),
  })
}
