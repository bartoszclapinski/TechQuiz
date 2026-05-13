using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
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
            throw new UnauthorizedAccessException(
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

        return BuildResult(attempt, quiz, score);
    }

    private static QuizResultDto BuildResult(QuizAttempt attempt, Quiz quiz, Score score)
    {
        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        var questionResults = quiz.Questions
            .Select(q =>
            {
                var userAnswer = answersByQuestion.GetValueOrDefault(q.Id);
                var isCorrect = userAnswer is not null
                    && userAnswer.SelectedOptionId.HasValue
                    && q.Options.Any(o => o.Id == userAnswer.SelectedOptionId.Value && o.IsCorrect);

                var options = q.Options
                    .Select(o => new OptionResultDto(o.Id, o.Text, o.OrderIndex, o.IsCorrect))
                    .ToList();

                return new QuestionResultDto(
                    q.Id,
                    q.Text,
                    q.Difficulty,
                    q.Explanation,
                    options,
                    userAnswer?.SelectedOptionId,
                    isCorrect);
            })
            .ToList();

        var byDifficulty = score.ByDifficulty.ToDictionary(
            kvp => kvp.Key,
            kvp => new DifficultyBreakdownDto(kvp.Value.Correct, kvp.Value.Total));

        return new QuizResultDto(
            attempt.Id,
            attempt.StartedAt,
            attempt.CompletedAt!.Value,
            score.CorrectCount,
            score.TotalCount,
            score.Percentage,
            byDifficulty,
            questionResults);
    }
}
