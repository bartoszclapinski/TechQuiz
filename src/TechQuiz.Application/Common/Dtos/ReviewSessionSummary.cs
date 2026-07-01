namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// A read projection of one completed review session: when it finished and how many of its questions
/// were answered (and answered correctly). Correctness is derived in the repository by matching the
/// chosen option against the question's options. Feeds <c>GetReviewStatsQuery</c>.
/// </summary>
public sealed record ReviewSessionSummary(
    DateTimeOffset CompletedAt,
    int QuestionCount,
    int CorrectCount);
