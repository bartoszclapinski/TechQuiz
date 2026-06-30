using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Review;

public sealed class GradeReviewCommandHandler(
    IQuizRepository quizRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<GradeReviewCommand, IReadOnlyList<ReviewGradeResultDto>>
{
    public async Task<IReadOnlyList<ReviewGradeResultDto>> Handle(
        GradeReviewCommand request,
        CancellationToken cancellationToken)
    {
        var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
        var grading = await quizRepository.GetQuestionsForGradingByIdsAsync(questionIds, cancellationToken);
        var byId = grading.ToDictionary(g => g.Id);

        var results = new List<ReviewGradeResultDto>(request.Answers.Count);
        var items = new List<ReviewItem>(request.Answers.Count);
        foreach (var answer in request.Answers)
        {
            // A question that vanished between fetch and grade is dropped rather than faked as wrong —
            // and excluded from the persisted session so we never write a dangling question reference.
            if (!byId.TryGetValue(answer.QuestionId, out var question))
            {
                continue;
            }

            results.Add(new ReviewGradeResultDto(
                answer.QuestionId,
                answer.SelectedOptionId,
                question.CorrectOptionId,
                answer.SelectedOptionId == question.CorrectOptionId,
                question.Explanation));

            items.Add(new ReviewItem(answer.QuestionId, answer.SelectedOptionId));
        }

        // Persist the session so it leaves a trace, feeds the spaced-repetition queue, and counts
        // toward review stats (ADR-021). Stored in its own table — never as a QuizAttempt.
        if (items.Count > 0)
        {
            var session = ReviewSession.Create(
                Guid.NewGuid(), userContext.UserId, timeProvider.GetUtcNow(), items);
            await quizRepository.AddReviewSessionAsync(session, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return results;
    }
}
