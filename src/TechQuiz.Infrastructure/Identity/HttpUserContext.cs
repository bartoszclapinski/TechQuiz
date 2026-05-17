using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Infrastructure.Identity;

/// <summary>
/// Resolves the current user's id from the active <see cref="HttpContext"/>'s JWT claims.
/// Implements <see cref="IUserContext"/> for the Application layer.
/// </summary>
/// <remarks>
/// Reads <see cref="ClaimTypes.NameIdentifier"/>, which the JWT bearer middleware maps
/// from the token's <c>sub</c> claim. Parses the value as <see cref="Guid"/> — matches
/// <c>ApplicationUser : IdentityUser&lt;Guid&gt;</c> so no string-to-Guid conversion drift.
///
/// Throws <see cref="InvalidOperationException"/> when invoked outside an authenticated
/// request. Handlers should only consult <see cref="UserId"/> from <c>[Authorize]</c>-
/// protected endpoints — failing loudly surfaces a missing authorization attribute early
/// instead of letting queries scoped by user id silently return everything.
/// </remarks>
public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException(
                    "IUserContext.UserId was accessed outside an HTTP request. Make sure the " +
                    "endpoint is reached only after JWT authentication has populated HttpContext.User.");

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException(
                    "Current user has no NameIdentifier claim. The endpoint must require [Authorize].");

            return Guid.TryParse(claim, out var id)
                ? id
                : throw new InvalidOperationException(
                    $"NameIdentifier claim '{claim}' is not a valid Guid — ApplicationUser uses IdentityUser<Guid>.");
        }
    }
}
