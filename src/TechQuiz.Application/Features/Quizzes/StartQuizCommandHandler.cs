using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class StartQuizCommandHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<StartQuizCommand, QuizSessionDto>
{
    public async Task<QuizSessionDto> Handle(StartQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByCategoryAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No quiz found for category {request.CategoryId}.");

        var attempt = QuizAttempt.Start(
            id: Guid.NewGuid(),
            userId: userContext.UserId,
            quizId: quiz.Id,
            startedAt: timeProvider.GetUtcNow());

        await quizRepository.AddAttemptAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var questionDtos = quiz.Questions
            .Select(q => new QuestionDto(
                q.Id,
                q.Type,
                q.Difficulty,
                q.Text,
                q.Options
                    .Select(o => new OptionDto(o.Id, o.Text, o.OrderIndex))
                    .ToList()))
            .ToList();

        return new QuizSessionDto(attempt.Id, questionDtos);
    }
}
