import { apiClient } from '../../lib/api-client'

// Mirrors the API's CodeChallengeDto. Difficulty is a name ("Easy"/"Medium"/"Hard"),
// consistent with the rest of the app. The hidden test cases are never sent (ADR-018 /
// hard rule #4) — only the prompt and starter code reach the client.
export type CodeChallenge = {
  id: string
  title: string
  difficulty: string
  prompt: string
  starterCode: string | null
}

// Mirrors the API's CodeExecutionResult — the raw sandbox verdict for an ad-hoc "Run".
export type CodeExecutionResult = {
  status: string
  stdout: string | null
  stderr: string | null
  compileOutput: string | null
  timeSeconds: number | null
  memoryKb: number | null
}

// One hidden case's outcome. Exposes only the user's own output — never the case's
// stdin or expected stdout (the grading harness stays hidden).
export type CodeChallengeCaseResult = {
  orderIndex: number
  passed: boolean
  status: string
  actualStdout: string | null
  stderr: string | null
  compileOutput: string | null
}

// Mirrors CodeChallengeGradeResult. Grading is two-stage: `compiled` gates `cases`.
// When compilation fails, `compiled` is false, `compileOutput` carries the diagnostics,
// and `cases` is empty — the UI must branch on `compiled`, not infer it.
export type CodeChallengeGradeResult = {
  compiled: boolean
  compileOutput: string | null
  passed: boolean
  passedCount: number
  totalCount: number
  cases: CodeChallengeCaseResult[]
}

export async function fetchCodeChallenges(): Promise<CodeChallenge[]> {
  const { data } = await apiClient.get<CodeChallenge[]>('/api/code-challenges')
  return data
}

export async function runCode(
  sourceCode: string,
  stdin: string | null,
): Promise<CodeExecutionResult> {
  const { data } = await apiClient.post<CodeExecutionResult>('/api/code/run', {
    sourceCode,
    stdin,
  })
  return data
}

export async function gradeChallenge(
  id: string,
  sourceCode: string,
): Promise<CodeChallengeGradeResult> {
  const { data } = await apiClient.post<CodeChallengeGradeResult>(
    `/api/code-challenges/${id}/grade`,
    { sourceCode },
  )
  return data
}

// Mirrors the API's FeedbackResponse. Qualitative AI prose only — complementary to the grade,
// never a score (ADR-018). `provider` is the enum name that produced it.
export type CodeFeedbackResult = {
  feedback: string
  provider: string
}

export async function getCodeFeedback(
  id: string,
  sourceCode: string,
  provider: string,
): Promise<CodeFeedbackResult> {
  const { data } = await apiClient.post<CodeFeedbackResult>(
    `/api/code-challenges/${id}/feedback`,
    { sourceCode, provider },
  )
  return data
}
