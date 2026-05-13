using FluentValidation;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
{
    public StartQuizCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required.");
    }
}
