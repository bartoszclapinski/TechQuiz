using FluentValidation;

namespace TechQuiz.Application.Features.Ai;

public sealed class SetAiKeyCommandValidator : AbstractValidator<SetAiKeyCommand>
{
    public SetAiKeyCommandValidator()
    {
        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage("Provider must be a known value.");

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithMessage("ApiKey is required.");
    }
}
