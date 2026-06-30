namespace TechQuiz.Domain;

/// <summary>
/// One occurrence of the user answering a question, used by <see cref="ReviewSelector"/> to build the
/// daily review queue. Multiple candidates may share a <see cref="QuestionId"/> (the question was
/// answered across several attempts); the selector keeps the latest per question.
/// </summary>
public sealed record ReviewCandidate(
    Guid QuestionId,
    Difficulty Difficulty,
    DateTimeOffset LastAnsweredAt,
    bool WasCorrect);
