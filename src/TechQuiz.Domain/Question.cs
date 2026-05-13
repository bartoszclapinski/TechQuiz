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

    private Question(
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

    public static Question Create(
        Guid id,
        Guid categoryId,
        QuestionType type,
        Difficulty difficulty,
        string text,
        string explanation,
        IReadOnlyList<Option> options)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidQuestionException("Question text must not be empty.");
        }

        if (options is null || options.Count < 2)
        {
            throw new InvalidQuestionException("Question must have at least 2 options.");
        }

        if (type == QuestionType.MultipleChoice)
        {
            var correctCount = options.Count(o => o.IsCorrect);
            if (correctCount != 1)
            {
                throw new InvalidQuestionException(
                    "MultipleChoice question must have exactly one correct option.");
            }
        }

        return new Question(id, categoryId, type, difficulty, text, explanation, options);
    }
}
