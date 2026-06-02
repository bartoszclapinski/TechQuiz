namespace TechQuiz.Application;

/// <summary>
/// Raised when an authenticated user tries to act on a resource they do not own.
/// Distinct from <see cref="UnauthorizedAccessException"/> (which the API maps to 401 for
/// failed authentication): identity is known and valid, but access is forbidden — the API
/// maps this to 403.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message) { }
}
