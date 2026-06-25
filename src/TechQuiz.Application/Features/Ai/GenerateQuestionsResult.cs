using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Drafts produced for a generation request, tagged with the provider that
/// produced them. Drafts are not persisted here — that is iteration 3.5
/// (public pool). Correct-answer indices stay server-side.
/// </summary>
public sealed record GenerateQuestionsResult(
    AiProviderKind Provider,
    IReadOnlyList<GeneratedQuestionDraft> Questions);
