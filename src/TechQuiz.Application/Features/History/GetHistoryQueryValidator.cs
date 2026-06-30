using FluentValidation;

namespace TechQuiz.Application.Features.History;

public sealed class GetHistoryQueryValidator : AbstractValidator<GetHistoryQuery>
{
    public GetHistoryQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
