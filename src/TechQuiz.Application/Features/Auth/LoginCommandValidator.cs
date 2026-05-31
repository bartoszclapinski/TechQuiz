using FluentValidation;

namespace TechQuiz.Application.Features.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        // Login does not validate password shape — that is the server-side credential
        // check's job. Only block the obviously-empty case here.
        RuleFor(c => c.Password)
            .NotEmpty();
    }
}
