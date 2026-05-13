using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

public interface IQuizRepository
{
    Task<Quiz?> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken = default);

    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuizAttempt>> GetAttemptsByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
