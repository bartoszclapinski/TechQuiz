using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Track>> GetTracksAsync(CancellationToken cancellationToken = default) =>
        await db.Tracks
            .AsNoTracking()
            .OrderBy(t => t.Position)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Position)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> GetQuestionCountsAsync(CancellationToken cancellationToken = default) =>
        await db.Questions
            .AsNoTracking()
            .GroupBy(q => q.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

    /// <summary>
    /// Best score percentage per category for the user, aggregated from the denormalised
    /// <see cref="Domain.QuizAttempt.ScorePercentage"/>. Categories the user has not completed
    /// an attempt in are absent from the result (caller treats them as 0%).
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, double>> GetUserBestScoresAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await db.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.ScorePercentage != null)
            .Join(
                db.Quizzes,
                attempt => attempt.QuizId,
                quiz => quiz.Id,
                (attempt, quiz) => new { quiz.CategoryId, attempt.ScorePercentage })
            .GroupBy(x => x.CategoryId)
            .Select(g => new { CategoryId = g.Key, Best = g.Max(x => x.ScorePercentage!.Value) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Best, cancellationToken);
}
