using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Pool;

/// <summary>
/// Projects published pool questions to DTOs, ordering options by their index and dropping the
/// correctness flag so the answer key never reaches a client (hard rule #4).
/// </summary>
public sealed class ListPooledQuestionsQueryHandler(IPooledQuestionRepository pooledQuestions)
    : IRequestHandler<ListPooledQuestionsQuery, IReadOnlyList<PooledQuestionDto>>
{
    public async Task<IReadOnlyList<PooledQuestionDto>> Handle(
        ListPooledQuestionsQuery request, CancellationToken cancellationToken)
    {
        var published = await pooledQuestions.GetPublishedAsync(cancellationToken);

        return published
            .Select(q => new PooledQuestionDto(
                q.Id,
                q.Text,
                q.Options.OrderBy(o => o.OrderIndex).Select(o => o.Text).ToList(),
                q.Difficulty,
                q.Explanation,
                q.Provider,
                q.Topic,
                q.CreatedByUserId,
                q.GeneratedAtUtc))
            .ToList();
    }
}
