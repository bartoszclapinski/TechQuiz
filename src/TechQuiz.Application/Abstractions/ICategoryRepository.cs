using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

public interface ICategoryRepository
{
    /// <summary>
    /// Returns all tracks (top-level groupings). Ordering is the caller's concern — the
    /// query handler sorts by <see cref="Domain.Track.Position"/>.
    /// </summary>
    Task<IReadOnlyList<Track>> GetTracksAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the question count keyed by category id, in one round-trip — avoids the N+1
    /// pattern that "count per category in a loop" would produce.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetQuestionCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user's best score percentage per category. Categories the user has
    /// not yet attempted are absent from the result (caller treats them as 0%).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, double>> GetUserBestScoresAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
