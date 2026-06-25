using FluentValidation;

namespace TechQuiz.Application.Features.Ai;

public sealed class RemoveAiKeyCommandValidator : AbstractValidator<RemoveAiKeyCommand>
{
    public RemoveAiKeyCommandValidator()
    {
        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage("Provider must be a known value.");
    }
}
