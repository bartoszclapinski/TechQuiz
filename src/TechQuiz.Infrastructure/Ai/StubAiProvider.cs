using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Deterministic <see cref="IAiProvider"/> with no network call. Introduced in iteration 3.1
/// to wire the provider seam before a real client existed; it is not registered in DI and
/// now serves only as a test double (<see cref="Kind"/> = <see cref="AiProviderKind.Anthropic"/>).
/// </summary>
public sealed class StubAiProvider : IAiProvider
{
    public AiProviderKind Kind => AiProviderKind.Anthropic;

    public Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
        GenerateQuestionsRequest request,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var drafts = Enumerable.Range(1, request.Count)
            .Select(n => new GeneratedQuestionDraft(
                Stem: $"[stub] {request.Topic} — question {n}",
                Options: ["Option A", "Option B", "Option C", "Option D"],
                CorrectOptionIndex: 0,
                Difficulty: request.Difficulty,
                Explanation: "Deterministic stub draft (no LLM call)."))
            .ToArray();

        return Task.FromResult<IReadOnlyList<GeneratedQuestionDraft>>(drafts);
    }

    public Task<string> GenerateCodeFeedbackAsync(
        CodeFeedbackRequest request,
        string apiKey,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The stub provider does not generate code feedback.");
}
