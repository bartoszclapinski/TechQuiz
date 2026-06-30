using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.History;
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

    /// <summary>
    /// Returns every completed, scored attempt of the given user, flattened with its category
    /// name and answer count, ordered oldest-to-newest by completion time. The dashboard
    /// aggregate (streak, averages, category strength, score-over-time) is computed from this
    /// in memory. In-progress attempts are excluded.
    /// </summary>
    Task<IReadOnlyList<CompletedAttemptRow>> GetCompletedAttemptsWithCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one page of the user's completed, scored attempts for the History screen,
    /// each flattened with its category name. Optionally filtered to a single
    /// <paramref name="category"/>, sorted server-side by date or score
    /// (<paramref name="sortBy"/> / <paramref name="descending"/>), then paginated
    /// (<paramref name="skip"/> / <paramref name="take"/>). In-progress attempts are excluded.
    /// </summary>
    Task<IReadOnlyList<HistoryItemDto>> GetCompletedHistoryPageAsync(
        Guid userId,
        string? category,
        HistorySortField sortBy,
        bool descending,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one <see cref="ReviewCandidate"/> per answer the user gave in a completed, scored
    /// attempt, carrying the question's difficulty, when it was answered, and whether it was correct
    /// (derived by matching the selected option to a correct one; an unanswered question counts as
    /// incorrect). A question answered across several attempts yields several candidates — the daily
    /// review selector keeps the latest per question. Feeds <see cref="ReviewSelector"/>.
    /// </summary>
    Task<IReadOnlyList<ReviewCandidate>> GetReviewCandidatesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the content (text, type, options, category name) of the given questions for the Daily
    /// review queue, in the in-quiz <see cref="ReviewQuestionDto"/> shape (no correctness leaked).
    /// Order is unspecified; the caller restores the review order.
    /// </summary>
    Task<IReadOnlyList<ReviewQuestionDto>> GetReviewQuestionsByIdsAsync(
        IReadOnlyCollection<Guid> questionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the grading projection (correct option id + explanation) for the given questions, used
    /// to score a submitted daily-review session. Unlike <see cref="GetReviewQuestionsByIdsAsync"/>
    /// this carries correctness, so it must never feed an active review — only the grade path.
    /// </summary>
    Task<IReadOnlyList<QuestionGradingDto>> GetQuestionsForGradingByIdsAsync(
        IReadOnlyCollection<Guid> questionIds,
        CancellationToken cancellationToken = default);
}
