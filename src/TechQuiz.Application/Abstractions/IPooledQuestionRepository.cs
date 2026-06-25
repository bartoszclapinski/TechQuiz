using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Persistence for the AI question pool (ADR-007). Drafts are added on generation; a single
/// draft is loaded for publishing; published questions are listed for browsing. The correct
/// option lives on the aggregate and is never projected to a client (hard rule #4).
/// </summary>
public interface IPooledQuestionRepository
{
    Task AddRangeAsync(IEnumerable<PooledQuestion> questions, CancellationToken cancellationToken = default);

    Task<PooledQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PooledQuestion>> GetPublishedAsync(CancellationToken cancellationToken = default);
}
