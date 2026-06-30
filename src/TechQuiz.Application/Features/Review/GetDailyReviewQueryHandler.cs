using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Review;

public sealed class GetDailyReviewQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    TimeProvider timeProvider)
    : IRequestHandler<GetDailyReviewQuery, IReadOnlyList<ReviewQuestionDto>>
{
    public async Task<IReadOnlyList<ReviewQuestionDto>> Handle(
        GetDailyReviewQuery request,
        CancellationToken cancellationToken)
    {
        var candidates = await quizRepository.GetReviewCandidatesAsync(
            userContext.UserId, cancellationToken);

        var selectedIds = ReviewSelector.SelectDailyReview(
            candidates, request.Count, timeProvider.GetUtcNow());

        if (selectedIds.Count == 0)
        {
            return [];
        }

        var questions = await quizRepository.GetReviewQuestionsByIdsAsync(
            selectedIds, cancellationToken);

        // The repository returns the chosen questions in arbitrary order; restore the selector's
        // weighting order (the questions to review first come first).
        var byId = questions.ToDictionary(q => q.Id);
        return selectedIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();
    }
}
