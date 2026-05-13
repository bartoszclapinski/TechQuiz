using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<int> CountQuestionsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user's best score percentage per category. Categories the user has
    /// not yet attempted are absent from the result (caller treats them as 0%).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, double>> GetUserBestScoresAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
