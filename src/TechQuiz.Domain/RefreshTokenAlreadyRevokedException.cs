namespace TechQuiz.Domain;

public sealed class RefreshTokenAlreadyRevokedException : DomainException
{
    public RefreshTokenAlreadyRevokedException()
        : base("Refresh token is already revoked.") { }

    public RefreshTokenAlreadyRevokedException(string message) : base(message) { }
}
