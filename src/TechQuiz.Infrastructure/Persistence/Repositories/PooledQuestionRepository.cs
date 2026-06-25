using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class PooledQuestionRepository(AppDbContext db) : IPooledQuestionRepository
{
    public async Task AddRangeAsync(
        IEnumerable<PooledQuestion> questions, CancellationToken cancellationToken = default) =>
        await db.PooledQuestions.AddRangeAsync(questions, cancellationToken);

    // Tracked (no AsNoTracking): the caller mutates and saves it (Publish).
    public Task<PooledQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.PooledQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PooledQuestion>> GetPublishedAsync(
        CancellationToken cancellationToken = default) =>
        await db.PooledQuestions
            .AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.Status == PooledQuestionStatus.Published)
            .OrderByDescending(q => q.GeneratedAtUtc)
            .ToListAsync(cancellationToken);
}
