using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
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
    /// <remarks>
    /// Intentionally does NOT scope the lookup by <see cref="QuizAttempt.UserId"/>.
    /// Ownership is a handler-level concern — every consuming handler
    /// (<c>SubmitAnswerCommandHandler</c>, <c>CompleteQuizCommandHandler</c>) is
    /// expected to compare <c>attempt.UserId</c> against the current
    /// <c>IUserContext.UserId</c> and throw <see cref="UnauthorizedAccessException"/>
    /// on mismatch before mutating. Calling this method without that guard would
    /// leak attempts across users.
    /// </remarks>
    public Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken = default) =>
        db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);

    /// <remarks>
    /// Uses synchronous <c>Add</c> rather than <c>AddAsync</c> because <c>QuizAttempt.Id</c>
    /// is supplied by the Domain (<c>QuizAttempt.Start(id, …)</c>) — there's no value
    /// generator that needs an async sequence read, so <c>AddAsync</c> would just wrap
    /// a synchronous operation in a task. The method is still <c>Task</c>-returning to
    /// match <c>IQuizRepository</c> and keep the call site uniform with future
    /// implementations that may legitimately need async work.
    /// </remarks>
    public Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken = default)
    {
        db.QuizAttempts.Add(attempt);
        return Task.CompletedTask;
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

    public Task<double?> GetLastCompletedScoreAsync(
        Guid userId,
        Guid quizId,
        Guid excludeAttemptId,
        CancellationToken cancellationToken = default) =>
        db.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.QuizId == quizId
                && a.Id != excludeAttemptId
                && a.CompletedAt != null
                && a.ScorePercentage != null)
            .OrderByDescending(a => a.CompletedAt)
            .Select(a => a.ScorePercentage)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<CompletedAttemptRow>> GetCompletedAttemptsWithCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await (
            from a in db.QuizAttempts.AsNoTracking()
            where a.UserId == userId && a.CompletedAt != null && a.ScorePercentage != null
            join q in db.Quizzes on a.QuizId equals q.Id
            join c in db.Categories on q.CategoryId equals c.Id
            orderby a.CompletedAt
            select new CompletedAttemptRow(
                a.Id,
                c.Name,
                a.ScorePercentage!.Value,
                a.CompletedAt!.Value,
                a.Answers.Count))
            .ToListAsync(cancellationToken);
}
