using FluentValidation;

namespace TechQuiz.Application.Features.Auth;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}
