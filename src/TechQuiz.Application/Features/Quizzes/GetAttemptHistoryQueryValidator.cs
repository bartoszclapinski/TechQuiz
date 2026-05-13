using FluentValidation;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class GetAttemptHistoryQueryValidator : AbstractValidator<GetAttemptHistoryQuery>
{
    public GetAttemptHistoryQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
