namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Commits pending repository changes to the persistence store as a single transaction.
/// Implemented in Infrastructure as a thin wrapper over <c>DbContext.SaveChangesAsync</c>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
