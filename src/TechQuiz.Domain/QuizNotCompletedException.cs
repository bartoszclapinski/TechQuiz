namespace TechQuiz.Domain;

public sealed class QuizNotCompletedException : DomainException
{
    public QuizNotCompletedException()
        : base("Quiz attempt is not completed yet — no result is available.") { }

    public QuizNotCompletedException(string message) : base(message) { }
}
