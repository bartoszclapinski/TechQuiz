namespace TechQuiz.Domain;

public class Question
{
    private readonly List<Option> _options = [];

    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public QuestionType Type { get; init; }
    public Difficulty Difficulty { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public IReadOnlyList<Option> Options => _options;

    // Parameterless constructor for EF Core materialisation.
    private Question() { }

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

        var question = new Question
        {
            Id = id,
            CategoryId = categoryId,
            Type = type,
            Difficulty = difficulty,
            Text = text,
            Explanation = explanation,
        };
        question._options.AddRange(options);
        return question;
    }
}
