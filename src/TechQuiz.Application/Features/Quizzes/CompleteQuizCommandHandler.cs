using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class CompleteQuizCommandHandler(
    IQuizRepository quizRepository,
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

        attempt.Complete(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var score = Score.Calculate(quiz.Questions, attempt.Answers);

        return QuizResultProjection.Build(attempt, quiz, score);
    }
}
