using FluentValidation;

namespace TechQuiz.Application.Features.Review;

public sealed class GradeReviewCommandValidator : AbstractValidator<GradeReviewCommand>
{
    public GradeReviewCommandValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty()
            .WithMessage("At least one answer is required.");

        When(x => x.Answers is not null, () =>
        {
            RuleFor(x => x.Answers)
                .Must(answers => answers.Count <= 50)
                .WithMessage("A review session has at most 50 answers.");

            RuleFor(x => x.Answers)
                .Must(answers => answers.Select(a => a.QuestionId).Distinct().Count() == answers.Count)
                .WithMessage("Duplicate questions are not allowed.");

            RuleForEach(x => x.Answers)
                .ChildRules(answer => answer.RuleFor(a => a.QuestionId).NotEmpty());
        });
    }
}
