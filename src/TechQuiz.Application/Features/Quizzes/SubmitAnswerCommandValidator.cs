using FluentValidation;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        // SelectedOptionId is intentionally nullable — null means "unanswered".
    }
}
