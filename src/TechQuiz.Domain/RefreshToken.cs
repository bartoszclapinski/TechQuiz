namespace TechQuiz.Domain;

public class RefreshToken
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public required string Token { get; init; }
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
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
