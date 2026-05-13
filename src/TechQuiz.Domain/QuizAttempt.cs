namespace TechQuiz.Domain;

public class QuizAttempt
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid QuizId { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private readonly List<Answer> _answers = [];
    public IReadOnlyList<Answer> Answers => _answers;

    public bool IsCompleted => CompletedAt.HasValue;

    private QuizAttempt(Guid id, Guid userId, Guid quizId, DateTimeOffset startedAt)
    {
        Id = id;
        UserId = userId;
        QuizId = quizId;
        StartedAt = startedAt;
    }

    public static QuizAttempt Start(Guid id, Guid userId, Guid quizId, DateTimeOffset startedAt)
    {
        return new QuizAttempt(id, userId, quizId, startedAt);
    }

    public void SubmitAnswer(Guid questionId, Guid? selectedOptionId, DateTimeOffset submittedAt)
    {
        if (IsCompleted)
        {
            throw new QuizAlreadyCompletedException(
                "Cannot submit answer — quiz attempt is already completed.");
        }

        var answer = new Answer(questionId, selectedOptionId, submittedAt);
        var existingIndex = _answers.FindIndex(a => a.QuestionId == questionId);
        if (existingIndex >= 0)
        {
            _answers[existingIndex] = answer;
        }
        else
        {
            _answers.Add(answer);
        }
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (IsCompleted)
        {
            throw new QuizAlreadyCompletedException();
        }

        CompletedAt = completedAt;
    }
}
