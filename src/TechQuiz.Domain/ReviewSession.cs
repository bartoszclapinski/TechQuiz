namespace TechQuiz.Domain;

/// <summary>
/// A completed daily-review run, persisted in its own right (never as a <see cref="QuizAttempt"/>) so
/// it leaves a trace, feeds the spaced-repetition queue, and earns its own stats — see ADR-021. Created
/// in one shot at grade time: a review collects answers locally and submits them all together.
/// </summary>
public class ReviewSession
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public DateTimeOffset CompletedAt { get; }

    private readonly List<ReviewItem> _items;
    public IReadOnlyList<ReviewItem> Items => _items;

    public int QuestionCount => _items.Count;

    private ReviewSession(Guid id, Guid userId, DateTimeOffset completedAt, List<ReviewItem> items)
    {
        Id = id;
        UserId = userId;
        CompletedAt = completedAt;
        _items = items;
    }

    public static ReviewSession Create(
        Guid id, Guid userId, DateTimeOffset completedAt, IEnumerable<ReviewItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var list = items.ToList();
        if (list.Count == 0)
        {
            throw new InvalidReviewSessionException(
                "A review session must contain at least one answered question.");
        }

        return new ReviewSession(id, userId, completedAt, list);
    }
}
