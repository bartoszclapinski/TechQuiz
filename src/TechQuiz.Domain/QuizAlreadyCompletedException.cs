namespace TechQuiz.Domain;

public sealed class QuizAlreadyCompletedException : DomainException
{
    public QuizAlreadyCompletedException()
        : base("Quiz attempt is already completed.") { }

    public QuizAlreadyCompletedException(string message) : base(message) { }
}
