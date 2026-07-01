namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A read projection of one completed review session: its id, when it finished, and how many of its
/// questions were answered (and answered correctly). Correctness is derived in the repository by
/// matching the chosen option against the question's options. Feeds <c>GetReviewStatsQuery</c> (which
/// ignores the id) and <c>GetReviewSessionsQuery</c> (the history list, which links on it).
/// </summary>
public sealed record ReviewSessionSummary(
    Guid Id,
    DateTimeOffset CompletedAt,
    int QuestionCount,
    int CorrectCount);
