using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Qualitative AI feedback on a submission, tagged with the provider that produced it (ADR-018).
/// This is prose only — complementary to, never a substitute for, the deterministic test verdict;
/// there is no score here because the grade owns pass/fail.
/// </summary>
public sealed record GetCodeFeedbackResult(string Feedback, AiProviderKind Provider);
