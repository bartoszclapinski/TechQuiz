using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Builds a fresh <see cref="RefreshToken"/> aggregate — random opaque token value plus
/// configured lifetime applied via the Domain factory. Lives in Infrastructure because it
/// needs both the crypto RNG and the <c>Jwt:RefreshTokenLifetimeDays</c> setting.
/// </summary>
public interface IRefreshTokenIssuer
{
    RefreshToken Issue(Guid userId, DateTimeOffset now);
}
