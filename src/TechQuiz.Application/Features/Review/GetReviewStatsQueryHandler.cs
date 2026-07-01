using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Review;

public sealed class GetReviewStatsQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    TimeProvider timeProvider)
    : IRequestHandler<GetReviewStatsQuery, ReviewStatsDto>
{
    public async Task<ReviewStatsDto> Handle(
        GetReviewStatsQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await quizRepository.GetReviewSessionSummariesAsync(
            userContext.UserId, cancellationToken);

        var totalSessions = summaries.Count;
        var totalQuestions = summaries.Sum(s => s.QuestionCount);
        var correct = summaries.Sum(s => s.CorrectCount);
        double? accuracy = totalQuestions > 0 ? (double)correct / totalQuestions * 100d : null;

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var activeDays = summaries
            .Select(s => DateOnly.FromDateTime(s.CompletedAt.UtcDateTime))
            .ToList();

        return new ReviewStatsDto(
            TotalSessions: totalSessions,
            TotalQuestionsReviewed: totalQuestions,
            AccuracyPercentage: accuracy,
            CurrentStreakDays: StreakCalculator.CurrentStreak(activeDays, today),
            BestStreakDays: StreakCalculator.LongestStreak(activeDays),
            ReviewedToday: activeDays.Contains(today));
    }
}
