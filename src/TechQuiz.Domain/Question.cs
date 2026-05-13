namespace TechQuiz.Domain;

public class Question
{
    public Guid Id { get; }
    public Guid CategoryId { get; }
    public QuestionType Type { get; }
    public Difficulty Difficulty { get; }
    public string Text { get; }
    public string Explanation { get; }
    public IReadOnlyList<Option> Options { get; }

    public Question(
        Guid id,
        Guid categoryId,
        QuestionType type,
        Difficulty difficulty,
        string text,
        string explanation,
        IReadOnlyList<Option> options)
    {
        Id = id;
        CategoryId = categoryId;
        Type = type;
        Difficulty = difficulty;
        Text = text;
        Explanation = explanation;
        Options = options;
    }
}
