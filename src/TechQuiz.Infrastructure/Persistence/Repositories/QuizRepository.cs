using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(AppDbContext db) : IQuizRepository
{
    public Task<Quiz?> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.CategoryId == categoryId, cancellationToken);

    public Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default) =>
        db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

    /// <summary>
    /// Returns the attempt as a tracked entity — the caller will mutate it via
    /// <c>SubmitAnswer</c> / <c>Complete</c> and then save via <c>IUnitOfWork</c>.
    /// Owned <c>Answers</c> are loaded automatically (they live in the same aggregate).
    /// </summary>
    public Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken = default) =>
        db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);

    public async Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken = default)
    {
        await db.QuizAttempts.AddAsync(attempt, cancellationToken);
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAttemptsByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        await db.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
}
