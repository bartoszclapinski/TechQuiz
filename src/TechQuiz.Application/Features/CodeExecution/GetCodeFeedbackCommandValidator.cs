using FluentValidation;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed class GetCodeFeedbackCommandValidator : AbstractValidator<GetCodeFeedbackCommand>
{
    public GetCodeFeedbackCommandValidator()
    {
        RuleFor(x => x.SourceCode)
            .NotEmpty()
            .WithMessage("Source code is required.");

        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage("Provider must be a known value.");
    }
}
