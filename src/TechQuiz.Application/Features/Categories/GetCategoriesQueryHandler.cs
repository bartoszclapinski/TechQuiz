using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Categories;

public sealed class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IUserContext userContext)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<TrackDto>>
{
    public async Task<IReadOnlyList<TrackDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var tracks = await categoryRepository.GetTracksAsync(cancellationToken);
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var questionCounts = await categoryRepository.GetQuestionCountsAsync(cancellationToken);
        var bestScores = await categoryRepository.GetUserBestScoresAsync(
            userContext.UserId, cancellationToken);

        var categoriesByTrack = categories
            .GroupBy(c => c.TrackId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Position).ToList());

        return tracks
            .OrderBy(t => t.Position)
            .Select(t => new TrackDto(
                t.Id,
                t.Name,
                t.Description,
                t.IconCode,
                t.Position,
                (categoriesByTrack.TryGetValue(t.Id, out var trackCategories)
                    ? trackCategories
                    : [])
                    .Select(c => new CategoryDto(
                        c.Id,
                        c.Name,
                        c.Description,
                        c.IconCode,
                        c.Position,
                        questionCounts.TryGetValue(c.Id, out var count) ? count : 0,
                        bestScores.TryGetValue(c.Id, out var score) ? score : 0d))
                    .ToList()))
            .ToList();
    }
}
