using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

public sealed class GradeReviewCommandHandler(IQuizRepository quizRepository)
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
        foreach (var answer in request.Answers)
        {
            // A question that vanished between fetch and grade is dropped rather than faked as wrong.
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
        }

        return results;
    }
}
