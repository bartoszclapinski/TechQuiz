namespace TechQuiz.Application;

/// <summary>
/// Raised by <see cref="Abstractions.IUserAccountService.CreateAsync"/> when Identity rejects
/// the create call (duplicate email, password policy violation, ...). <see cref="Errors"/>
/// carries the structured Identity error messages so the API can surface them via
/// ProblemDetails without losing context.
/// </summary>
public sealed class RegistrationFailedException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public RegistrationFailedException(IEnumerable<string> errors)
        : base("Registration failed: " + string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}
