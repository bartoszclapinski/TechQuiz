namespace TechQuiz.Domain;

public sealed class InvalidCodeChallengeException : DomainException
{
    public InvalidCodeChallengeException(string message) : base(message) { }
}
