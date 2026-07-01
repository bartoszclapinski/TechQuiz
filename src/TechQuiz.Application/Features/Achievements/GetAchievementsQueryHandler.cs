using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Achievements;

public sealed class GetAchievementsQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetAchievementsQuery, AchievementsDto>
{
    public async Task<AchievementsDto> Handle(
        GetAchievementsQuery request,
        CancellationToken cancellationToken)
    {
        var attempts = await quizRepository.GetCompletedAttemptsWithCategoryAsync(
            userContext.UserId, cancellationToken);
        var reviews = await quizRepository.GetReviewSessionSummariesAsync(
            userContext.UserId, cancellationToken);

        return AchievementCalculator.Calculate(attempts, reviews);
    }
}
