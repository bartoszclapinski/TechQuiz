using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Pool;

/// <summary>
/// A published pool question as shown when browsing (ADR-007). Carries attribution (author,
/// provider, generation timestamp) and the option texts, but **never** which option is correct
/// (hard rule #4) — the pool browse is a catalogue, not an answer key.
/// </summary>
public sealed record PooledQuestionDto(
    Guid Id,
    string Stem,
    IReadOnlyList<string> Options,
    Difficulty Difficulty,
    string? Explanation,
    string Provider,
    string Topic,
    Guid CreatedByUserId,
    DateTime GeneratedAtUtc);
