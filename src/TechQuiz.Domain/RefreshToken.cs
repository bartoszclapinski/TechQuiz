namespace TechQuiz.Domain;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // Parameterless constructor for EF Core materialisation.
    private RefreshToken() { }

    public static RefreshToken Issue(
        Guid id,
        Guid userId,
        string token,
        DateTimeOffset issuedAt,
        TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidRefreshTokenException("Refresh token value must not be empty.");
        }

        if (lifetime <= TimeSpan.Zero)
        {
            throw new InvalidRefreshTokenException("Refresh token lifetime must be positive.");
        }

        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            Token = token,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt + lifetime,
        };
    }

    public bool IsActiveAt(DateTimeOffset now) =>
        RevokedAt is null && now < ExpiresAt;

    public void Revoke(DateTimeOffset revokedAt)
    {
        // Revoke and expiry are independent — revoking an already-expired token is
        // legitimate (administrative bookkeeping), revoking an already-revoked one is not.
        if (RevokedAt is not null)
        {
            throw new RefreshTokenAlreadyRevokedException();
        }

        RevokedAt = revokedAt;
    }
}
