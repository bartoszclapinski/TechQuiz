using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

public interface IQuizRepository
{
    Task<Quiz?> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default);

    Task<QuizAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken cancellationToken = default);

    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuizAttempt>> GetAttemptsByUserAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the score percentage of the user's most recently completed attempt for the
    /// given quiz, excluding <paramref name="excludeAttemptId"/> (the one just finished).
    /// Null when the user has no earlier completed attempt for that quiz.
    /// </summary>
    Task<double?> GetLastCompletedScoreAsync(
        Guid userId,
        Guid quizId,
        Guid excludeAttemptId,
        CancellationToken cancellationToken = default);
}
