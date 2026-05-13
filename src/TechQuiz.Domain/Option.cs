namespace TechQuiz.Domain;

public class Option
{
    public Guid Id { get; }
    public Guid QuestionId { get; }
    public string Text { get; }
    public bool IsCorrect { get; }
    public int OrderIndex { get; }

    public Option(Guid id, Guid questionId, string text, bool isCorrect, int orderIndex)
    {
        Id = id;
        QuestionId = questionId;
        Text = text;
        IsCorrect = isCorrect;
        OrderIndex = orderIndex;
    }
}
