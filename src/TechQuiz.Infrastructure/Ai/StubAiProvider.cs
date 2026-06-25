using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Placeholder <see cref="IAiProvider"/> that returns deterministic drafts without
/// any network call. It exists so the provider seam is wired end-to-end during
/// iteration 3.1; iteration 3.2 replaces it with a real Anthropic-backed client
/// (hence <see cref="Kind"/> = <see cref="AiProviderKind.Anthropic"/>).
/// </summary>
public sealed class StubAiProvider : IAiProvider
{
    public AiProviderKind Kind => AiProviderKind.Anthropic;

    public Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
        GenerateQuestionsRequest request,
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
}
