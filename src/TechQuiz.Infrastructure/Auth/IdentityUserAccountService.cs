using Microsoft.AspNetCore.Identity;
using TechQuiz.Application;
using TechQuiz.Application.Abstractions;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Auth;

/// <summary>
/// Adapts <see cref="UserManager{TUser}"/> to the Application-layer <see cref="IUserAccountService"/>
/// abstraction so Application code stays free of any <c>Microsoft.AspNetCore.Identity</c> reference.
/// </summary>
public sealed class IdentityUserAccountService(UserManager<ApplicationUser> userManager) : IUserAccountService
{
    public async Task<Guid> CreateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        // UserManager APIs don't accept a CancellationToken — same gap as DataSeeder
        // works around. ThrowIfCancellationRequested keeps shutdown cooperative even
        // though the call itself can't be cancelled mid-flight.
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            // MVP: skip email confirmation. Phase 4 adds the confirmation flow.
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(e => e.Description));
        }

        return user.Id;
    }

    public async Task<Guid?> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var passwordValid = await userManager.CheckPasswordAsync(user, password);
        return passwordValid ? user.Id : null;
    }

    public async Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.Email is null)
        {
            return null;
        }

        return new UserAccount(user.Id, user.Email);
    }
}
