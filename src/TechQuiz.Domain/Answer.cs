namespace TechQuiz.Domain;

public class Answer
{
    public Guid QuestionId { get; }

    /// <summary>
    /// Id of the selected option. <c>null</c> means the question was left unanswered
    /// (counted as incorrect by the scoring rules).
    /// </summary>
    public Guid? SelectedOptionId { get; }

    public DateTimeOffset SubmittedAt { get; }

    public Answer(Guid questionId, Guid? selectedOptionId, DateTimeOffset submittedAt)
    {
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        SubmittedAt = submittedAt;
    }
}
