using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> GetQuestionCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await db.Questions
            .AsNoTracking()
            .GroupBy(q => q.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.CategoryId, x => x.Count);
    }

    /// <summary>
    /// MVP placeholder — returns an empty dictionary so <c>GetCategoriesQueryHandler</c>
    /// falls back to its <c>0d</c> default for every category.
    /// </summary>
    /// <remarks>
    /// Computing real best-scores per category requires loading every completed attempt's
    /// answers + the quiz's questions + their options and running <c>Score.Calculate</c>
    /// for each — heavy for a denormalisable metric. The proper fix is to persist the
    /// score percentage on <c>QuizAttempt</c> at <c>Complete</c> time and aggregate via a
    /// simple <c>MAX() GROUP BY</c> query. Tracked separately for when iteration 1.6
    /// (Categories UI) actually consumes this method.
    /// </remarks>
    public Task<IReadOnlyDictionary<Guid, double>> GetUserBestScoresAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<Guid, double>>(new Dictionary<Guid, double>());
}
