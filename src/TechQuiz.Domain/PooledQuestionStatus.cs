namespace TechQuiz.Domain;

/// <summary>
/// Lifecycle of a generated question in the pool (ADR-020). A question is created in
/// <see cref="Draft"/> (private to its author) and moves to <see cref="Published"/> when the
/// author shares it into the public pool. Moderation states (ADR-007) attach here later.
/// </summary>
public enum PooledQuestionStatus
{
    Draft,
    Published
}
