namespace TechQuiz.Domain;

public sealed class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException(string message) : base(message) { }
}
