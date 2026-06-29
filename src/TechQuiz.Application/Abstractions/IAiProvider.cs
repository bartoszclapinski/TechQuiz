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

    /// <summary>
    /// Returns qualitative prose feedback on a code-challenge submission (ADR-018) using the
    /// caller-supplied <paramref name="apiKey"/> (bring-your-own-key, ADR-006). This is
    /// explicitly <i>complementary</i> to the deterministic test verdict — it never decides
    /// pass/fail. The provider runs server-side and may receive the hidden test cases to reason
    /// about failures, but implementations must prompt the model to guide without quoting exact
    /// expected outputs, preserving the spirit of the hidden harness. Providers that do not
    /// support feedback may throw <see cref="NotSupportedException"/>.
    /// </summary>
    Task<string> GenerateCodeFeedbackAsync(
        CodeFeedbackRequest request,
        string apiKey,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Everything the model needs to critique a submission: the challenge's title and prompt, the
/// user's C# <paramref name="SourceCode"/>, and the hidden <paramref name="TestCases"/> it must
/// satisfy. The test cases are passed server-side for the model to reason about edge cases; the
/// provider prompt must forbid quoting their exact expected outputs back to the user.
/// </summary>
public sealed record CodeFeedbackRequest(
    string ChallengeTitle,
    string Prompt,
    string SourceCode,
    IReadOnlyList<CodeFeedbackTestCase> TestCases);

/// <summary>A hidden test case: the stdin fed to the program and the stdout it must produce.</summary>
public sealed record CodeFeedbackTestCase(string Stdin, string ExpectedStdout);

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
