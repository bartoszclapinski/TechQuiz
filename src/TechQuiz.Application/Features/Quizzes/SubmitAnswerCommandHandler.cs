using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class SubmitAnswerCommandHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<SubmitAnswerCommand>
{
    public async Task Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
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

        if (quiz.Questions.All(q => q.Id != request.QuestionId))
        {
            throw new ArgumentException(
                $"Question {request.QuestionId} does not belong to this attempt's quiz.",
                nameof(request));
        }

        attempt.SubmitAnswer(
            request.QuestionId,
            request.SelectedOptionId,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
