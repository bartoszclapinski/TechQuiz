namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Client-facing view of a <see cref="Domain.CodeChallenge"/>. Deliberately omits the
/// hidden test cases — exposing them would leak the grading harness (cf. the rule against
/// serving option correctness during a quiz).
/// </summary>
public sealed record CodeChallengeDto(
    Guid Id,
    string Title,
    string Difficulty,
    string Prompt,
    string? StarterCode);
