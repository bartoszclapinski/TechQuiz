using Microsoft.AspNetCore.Identity;

namespace TechQuiz.Infrastructure.Persistence.Identity;

/// <summary>
/// Identity user keyed by <see cref="Guid"/> — matches the type of <c>QuizAttempt.UserId</c> in the Domain
/// so that the FK relationship between attempts and users lines up without conversion.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
