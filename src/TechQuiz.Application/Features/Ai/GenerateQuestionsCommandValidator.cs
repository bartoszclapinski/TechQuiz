using FluentValidation;

namespace TechQuiz.Application.Features.Ai;

public sealed class GenerateQuestionsCommandValidator : AbstractValidator<GenerateQuestionsCommand>
{
    public GenerateQuestionsCommandValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty()
            .WithMessage("Topic is required.");

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20.");

        RuleFor(x => x.Difficulty)
            .IsInEnum()
            .WithMessage("Difficulty must be a known value.");

        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage("Provider must be a known value.");
    }
}
