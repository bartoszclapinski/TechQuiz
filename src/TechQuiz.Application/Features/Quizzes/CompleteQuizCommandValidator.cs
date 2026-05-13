using FluentValidation;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class CompleteQuizCommandValidator : AbstractValidator<CompleteQuizCommand>
{
    public CompleteQuizCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
    }
}
