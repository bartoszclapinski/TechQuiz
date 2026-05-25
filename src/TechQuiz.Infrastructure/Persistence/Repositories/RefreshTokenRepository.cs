using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    /// <summary>
    /// Returns a tracked entity — <c>RefreshCommandHandler</c> calls <c>Revoke(now)</c> on
    /// the result and relies on EF change tracking to persist the mutation via the
    /// shared <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    public Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        db.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }
}
