using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Generates draft quiz questions from a topic via an external LLM (ADR-006).
/// Implementations live in Infrastructure and are integration-tested (ADR-008);
/// Application code depends only on this port. Each implementation reports the
/// <see cref="AiProviderKind"/> it serves so the resolver can select it.
/// </summary>
public interface IAiProvider
{
    AiProviderKind Kind { get; }

    /// <summary>
    /// Generates drafts using the caller-supplied <paramref name="apiKey"/>. The key is
    /// the current user's own credential (bring-your-own-key, ADR-006); it is passed per
    /// call rather than held by the provider, which stays a stateless HTTP client and is
    /// safe to register as a singleton.
    /// </summary>
    Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
        GenerateQuestionsRequest request,
        string apiKey,
        CancellationToken cancellationToken = default);
}

/// <summary>What to generate: a topic, a target difficulty, and how many questions.</summary>
public sealed record GenerateQuestionsRequest(string Topic, Difficulty Difficulty, int Count);

/// <summary>
/// A single generated question before persistence. <paramref name="CorrectOptionIndex"/>
/// points into <paramref name="Options"/> and is server-side only — it is never serialized
/// to a client during a quiz (same rule as <c>QuestionDto</c> omitting option correctness).
/// </summary>
public sealed record GeneratedQuestionDraft(
    string Stem,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    Difficulty Difficulty,
    string? Explanation);
