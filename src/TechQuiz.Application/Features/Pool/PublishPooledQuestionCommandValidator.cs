using FluentValidation;

namespace TechQuiz.Application.Features.Pool;

public sealed class PublishPooledQuestionCommandValidator : AbstractValidator<PublishPooledQuestionCommand>
{
    public PublishPooledQuestionCommandValidator()
    {
        RuleFor(x => x.PooledQuestionId)
            .NotEmpty()
            .WithMessage("PooledQuestionId is required.");
    }
}
