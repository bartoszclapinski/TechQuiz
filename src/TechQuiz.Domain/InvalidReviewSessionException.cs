namespace TechQuiz.Domain;

public sealed class InvalidReviewSessionException : DomainException
{
    public InvalidReviewSessionException(string message) : base(message) { }
}
