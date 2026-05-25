using FluentValidation;

namespace TechQuiz.Application.Features.Auth;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        // First-cut shape check — Identity's PasswordOptions enforces the full policy
        // (length, character classes) and surfaces structured errors via
        // RegistrationFailedException. Validator just blocks the obviously-empty case
        // before the round-trip to UserManager.
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
