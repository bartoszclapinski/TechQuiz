namespace TechQuiz.Domain;

public class Quiz
{
    public Guid Id { get; }
    public Guid CategoryId { get; }
    public IReadOnlyList<Question> Questions { get; }

    private Quiz(Guid id, Guid categoryId, IReadOnlyList<Question> questions)
    {
        Id = id;
        CategoryId = categoryId;
        Questions = questions;
    }

    public static Quiz Create(Guid id, Guid categoryId, IReadOnlyList<Question> questions)
    {
        if (questions is null || questions.Count == 0)
        {
            throw new ArgumentException("Quiz must have at least 1 question.", nameof(questions));
        }

        return new Quiz(id, categoryId, questions);
    }
}
