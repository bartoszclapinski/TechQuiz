namespace TechQuiz.Domain;

public sealed class InvalidQuestionException : DomainException
{
    public InvalidQuestionException(string message) : base(message) { }
}
