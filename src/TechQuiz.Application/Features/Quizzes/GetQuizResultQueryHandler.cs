using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class GetQuizResultQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetQuizResultQuery, QuizResultDto>
{
    public async Task<QuizResultDto> Handle(GetQuizResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await quizRepository.GetAttemptAsync(request.AttemptId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Attempt {request.AttemptId} not found.");

        if (attempt.UserId != userContext.UserId)
        {
            throw new UnauthorizedAccessException(
                "Attempt does not belong to the current user.");
        }

        if (!attempt.IsCompleted)
        {
            throw new QuizNotCompletedException();
        }

        var quiz = await quizRepository.GetByIdAsync(attempt.QuizId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Attempt {attempt.Id} references missing quiz {attempt.QuizId}.");

        var score = Score.Calculate(quiz.Questions, attempt.Answers);

        return QuizResultProjection.Build(attempt, quiz, score);
    }
}
