namespace TechQuiz.Domain;

public sealed class InvalidPooledQuestionException : DomainException
{
    public InvalidPooledQuestionException(string message) : base(message) { }
}
