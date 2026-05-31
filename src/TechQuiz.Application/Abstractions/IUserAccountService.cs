namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Wraps the parts of ASP.NET Identity's <c>UserManager</c> that auth use cases need.
/// Implemented in Infrastructure — Application stays free of any <c>Microsoft.AspNetCore.Identity</c>
/// reference so the dependency rule (ADR-001) holds.
/// </summary>
public interface IUserAccountService
{
    /// <summary>
    /// Creates a new user with the given credentials. Throws
    /// <see cref="TechQuiz.Application.RegistrationFailedException"/> on Identity policy
    /// violations or duplicate emails.
    /// </summary>
    Task<Guid> CreateAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the matched account (id + canonical stored email) when the email+password
    /// pair matches, <c>null</c> otherwise. Callers must not distinguish "no such user"
    /// from "wrong password" in the response — the single null signal keeps the API from
    /// leaking which field was wrong. The returned email is the stored value, not the
    /// caller's input, so downstream token claims are independent of input casing.
    /// </summary>
    Task<UserAccount?> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public sealed record UserAccount(Guid Id, string Email);
