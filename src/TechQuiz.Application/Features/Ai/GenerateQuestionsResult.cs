using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Drafts produced for a generation request, tagged with the provider that produced them.
/// As of iteration 3.5 each draft is persisted to the pool in <see cref="PooledQuestionStatus.Draft"/>
/// (ADR-020), so every item carries its server-side <see cref="GeneratedQuestionSummary.Id"/> — the
/// handle the client uses to publish it. The correct-answer index stays server-side and is not here.
/// </summary>
public sealed record GenerateQuestionsResult(
    AiProviderKind Provider,
    IReadOnlyList<GeneratedQuestionSummary> Questions);

/// <summary>
/// A persisted draft as returned to the caller: its pool id plus the answer-key-free fields the
/// preview shows. <see cref="Id"/> identifies the draft for a later publish; the correct option is
/// never included (hard rule #4).
/// </summary>
public sealed record GeneratedQuestionSummary(
    Guid Id,
    string Stem,
    IReadOnlyList<string> Options,
    Difficulty Difficulty,
    string? Explanation);
