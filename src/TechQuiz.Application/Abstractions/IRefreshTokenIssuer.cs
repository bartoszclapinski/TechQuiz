using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Builds a fresh refresh token — a random opaque secret plus the aggregate that persists
/// only its hash. Lives in Infrastructure because it needs both the crypto RNG and the
/// <c>Jwt:RefreshTokenLifetimeDays</c> setting.
/// </summary>
public interface IRefreshTokenIssuer
{
    IssuedRefreshToken Issue(Guid userId, DateTimeOffset now);
}

/// <summary>
/// Pairs the persistable <see cref="RefreshToken"/> aggregate (which holds only the hash)
/// with the one-time <see cref="RawValue"/> handed to the client. The raw value exists only
/// here and in the HTTP response — it is never persisted.
/// </summary>
public sealed record IssuedRefreshToken(RefreshToken Token, string RawValue);
