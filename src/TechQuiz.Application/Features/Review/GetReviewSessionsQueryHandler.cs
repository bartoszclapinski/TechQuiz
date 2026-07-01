using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

public sealed class GetReviewSessionsQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetReviewSessionsQuery, IReadOnlyList<ReviewSessionSummary>>
{
    public async Task<IReadOnlyList<ReviewSessionSummary>> Handle(
        GetReviewSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await quizRepository.GetReviewSessionSummariesAsync(
            userContext.UserId, cancellationToken);

        return [.. summaries.OrderByDescending(s => s.CompletedAt)];
    }
}
