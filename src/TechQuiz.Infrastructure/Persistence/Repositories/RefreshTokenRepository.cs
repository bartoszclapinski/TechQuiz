using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Auth;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    /// <summary>
    /// Returns a tracked entity — <c>RefreshCommandHandler</c> calls <c>Revoke(now)</c> on
    /// the result and relies on EF change tracking to persist the mutation via the
    /// shared <c>IUnitOfWork.SaveChangesAsync</c>. The raw client value is hashed before
    /// lookup; only the hash is stored.
    /// </summary>
    public Task<RefreshToken?> FindByTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var hash = RefreshTokenHasher.Hash(rawToken);
        return db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        db.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }
}
