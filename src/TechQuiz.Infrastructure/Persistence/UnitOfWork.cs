using TechQuiz.Application.Abstractions;

namespace TechQuiz.Infrastructure.Persistence;

/// <summary>
/// Thin <see cref="IUnitOfWork"/> wrapper over <see cref="AppDbContext.SaveChangesAsync"/>.
/// Exists so the Application layer can commit a transaction without depending on
/// <c>Microsoft.EntityFrameworkCore</c> directly — keeps ADR-001's "Application
/// references only Domain + abstractions" boundary clean.
/// </summary>
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
