using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Categories;

public sealed class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IUserContext userContext)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var questionCounts = await categoryRepository.GetQuestionCountsAsync(cancellationToken);
        var bestScores = await categoryRepository.GetUserBestScoresAsync(
            userContext.UserId, cancellationToken);

        return categories
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IconCode,
                questionCounts.TryGetValue(c.Id, out var count) ? count : 0,
                bestScores.TryGetValue(c.Id, out var score) ? score : 0d))
            .ToList();
    }
}
