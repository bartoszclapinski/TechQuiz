using FluentValidation;

namespace TechQuiz.Application.Features.Review;

public sealed class GetDailyReviewQueryValidator : AbstractValidator<GetDailyReviewQuery>
{
    public GetDailyReviewQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 50)
            .WithMessage("Count must be between 1 and 50.");
    }
}
