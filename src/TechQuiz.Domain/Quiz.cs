namespace TechQuiz.Domain;

public class Quiz
{
    private readonly List<Question> _questions = [];

    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public IReadOnlyList<Question> Questions => _questions;

    // Parameterless constructor for EF Core materialisation.
    private Quiz() { }

    public static Quiz Create(Guid id, Guid categoryId, IReadOnlyList<Question> questions)
    {
        if (questions is null || questions.Count == 0)
        {
            throw new ArgumentException("Quiz must have at least 1 question.", nameof(questions));
        }

        var quiz = new Quiz
        {
            Id = id,
            CategoryId = categoryId,
        };
        quiz._questions.AddRange(questions);
        return quiz;
    }
}
