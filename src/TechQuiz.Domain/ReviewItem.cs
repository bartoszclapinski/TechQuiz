namespace TechQuiz.Domain;

/// <summary>
/// One answered question within a <see cref="ReviewSession"/>. Correctness is not stored — it is
/// derived at read time by matching <see cref="SelectedOptionId"/> against the question's options,
/// the same way quiz scoring works. <c>null</c> means the question was left unanswered.
/// </summary>
public class ReviewItem
{
    public Guid QuestionId { get; }
    public Guid? SelectedOptionId { get; }

    public ReviewItem(Guid questionId, Guid? selectedOptionId)
    {
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
    }
}
