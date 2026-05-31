using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Looks up a stored token by its raw client value. The implementation hashes
    /// <paramref name="rawToken"/> and matches on the stored hash — the raw value is
    /// never persisted, so callers always pass the value as received from the client.
    /// </summary>
    Task<RefreshToken?> FindByTokenAsync(string rawToken, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
