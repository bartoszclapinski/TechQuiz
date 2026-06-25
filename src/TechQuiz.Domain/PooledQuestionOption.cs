namespace TechQuiz.Domain;

/// <summary>
/// An answer option on a <see cref="PooledQuestion"/>. <see cref="IsCorrect"/> is server-side
/// only — it is never serialized to a client (hard rule #4), in either pool status.
/// </summary>
public class PooledQuestionOption
{
    public Guid Id { get; }
    public string Text { get; }
    public bool IsCorrect { get; }
    public int OrderIndex { get; }

    public PooledQuestionOption(Guid id, string text, bool isCorrect, int orderIndex)
    {
        Id = id;
        Text = text;
        IsCorrect = isCorrect;
        OrderIndex = orderIndex;
    }
}
