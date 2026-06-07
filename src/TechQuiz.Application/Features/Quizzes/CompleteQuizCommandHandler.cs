using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class CompleteQuizCommandHandler(
    IQuizRepository quizRepository,
    ICategoryRepository categoryRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteQuizCommand, QuizResultDto>
{
    public async Task<QuizResultDto> Handle(CompleteQuizCommand request, CancellationToken cancellationToken)
    {
        var attempt = await quizRepository.GetAttemptAsync(request.AttemptId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Attempt {request.AttemptId} not found.");

        if (attempt.UserId != userContext.UserId)
        {
            throw new ForbiddenAccessException(
                "Attempt does not belong to the current user.");
        }

        if (attempt.IsCompleted)
        {
            throw new QuizAlreadyCompletedException();
        }

        var quiz = await quizRepository.GetByIdAsync(attempt.QuizId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Attempt {attempt.Id} references missing quiz {attempt.QuizId}.");

        var score = Score.Calculate(quiz.Questions, attempt.Answers);
        attempt.Complete(timeProvider.GetUtcNow(), score.Percentage);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryName = categories.FirstOrDefault(c => c.Id == quiz.CategoryId)?.Name ?? string.Empty;

        // Best now includes the just-saved attempt; previous excludes it to give the prior score.
        var bestScores = await categoryRepository.GetUserBestScoresAsync(userContext.UserId, cancellationToken);
        var bestPercentage = bestScores.TryGetValue(quiz.CategoryId, out var best) ? best : score.Percentage;

        var previousPercentage = await quizRepository.GetLastCompletedScoreAsync(
            userContext.UserId, attempt.QuizId, attempt.Id, cancellationToken);

        return QuizResultProjection.Build(attempt, quiz, score, categoryName, bestPercentage, previousPercentage);
    }
}
