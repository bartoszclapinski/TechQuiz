using System.Text.Json;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Provider-neutral prompt text and reply parsing shared by every <see cref="IAiProvider"/>.
/// Providers own only their HTTP call; what is asked and how the answer is read lives here,
/// so all providers produce the same drafts and the same feedback guidance.
/// </summary>
internal static class AiPrompts
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public static string BuildQuestionsPrompt(GenerateQuestionsRequest request) =>
        $$"""
        Generate {{request.Count}} multiple-choice quiz question(s) about "{{request.Topic}}"
        at {{request.Difficulty}} difficulty for a developer self-assessment.

        Respond with ONLY a JSON object (no prose, no markdown fences) of this shape:
        {
          "questions": [
            {
              "stem": "the question text",
              "options": ["four", "distinct", "answer", "choices"],
              "correctOptionIndex": 0,
              "explanation": "one sentence explaining the correct answer"
            }
          ]
        }

        Exactly four options per question. correctOptionIndex is the 0-based index of the
        correct option.
        """;

    public static string BuildFeedbackPrompt(CodeFeedbackRequest request)
    {
        // The hidden cases are given so the model can reason about edge cases the submission
        // misses, but it must NOT quote the exact expected outputs back — that would leak the
        // harness (spirit of hard rule #4). Guidance is qualitative: "you don't handle empty
        // input", not "test 2 expects 30".
        var cases = string.Join(
            "\n",
            request.TestCases.Select((c, i) =>
                $"  Case {i + 1}: stdin={Quote(c.Stdin)} expectedStdout={Quote(c.ExpectedStdout)}"));

        return $$"""
        You are a senior C# reviewer giving feedback to a developer on a coding exercise.

        Challenge: {{request.ChallengeTitle}}
        Task: {{request.Prompt}}

        The developer's submission:
        ```csharp
        {{request.SourceCode}}
        ```

        Hidden test cases (for your reasoning only):
        {{cases}}

        Write concise, constructive feedback in markdown prose: what the code does well, what is
        risky or incorrect, and which edge cases it may miss. Be specific about the code, but do
        NOT quote or restate the exact expected outputs above — describe the behaviour the code
        should have ("handle empty input", "trim trailing whitespace") rather than the literal
        expected values. Do not include a numeric score; the automated tests already decide pass
        or fail. Keep it under ~200 words.
        """;
    }

    /// <summary>
    /// Parses a model reply into drafts at the requested difficulty. The contract is a
    /// <c>{ "questions": [...] }</c> object (strict JSON-schema modes need an object root); a bare
    /// array is still accepted because models do not always follow the prompt.
    /// <paramref name="providerName"/> only labels the <see cref="AiResponseException"/> message.
    /// </summary>
    public static IReadOnlyList<GeneratedQuestionDraft> ParseDrafts(
        string text, Difficulty difficulty, string providerName)
    {
        var json = StripJsonFences(text);

        DraftDto[]? drafts;
        try
        {
            drafts = json.StartsWith('[')
                ? JsonSerializer.Deserialize<DraftDto[]>(json, JsonOptions)
                : JsonSerializer.Deserialize<DraftsEnvelope>(json, JsonOptions)?.Questions;
        }
        catch (JsonException ex)
        {
            throw new AiResponseException($"{providerName} response was not valid question JSON: {ex.Message}");
        }

        if (drafts is null || drafts.Length == 0)
        {
            throw new AiResponseException($"{providerName} response contained no questions.");
        }

        return drafts
            .Select(d => new GeneratedQuestionDraft(
                d.Stem, d.Options, d.CorrectOptionIndex, difficulty, d.Explanation))
            .ToArray();
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value);

    // Models sometimes wrap JSON in ```json … ``` despite instructions; tolerate it.
    private static string StripJsonFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        var withoutOpen = firstNewline >= 0 ? trimmed[(firstNewline + 1)..] : trimmed;
        var closingFence = withoutOpen.LastIndexOf("```", StringComparison.Ordinal);
        return (closingFence >= 0 ? withoutOpen[..closingFence] : withoutOpen).Trim();
    }

    private sealed record DraftsEnvelope(DraftDto[]? Questions);

    private sealed record DraftDto(
        string Stem,
        IReadOnlyList<string> Options,
        int CorrectOptionIndex,
        string? Explanation);
}
