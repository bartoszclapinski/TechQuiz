namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Issues short-lived signed access tokens. Lifetime + signing key live in the
/// Infrastructure implementation (read from <c>Jwt:*</c> configuration).
/// </summary>
public interface IJwtTokenService
{
    AccessTokenResult IssueAccessToken(Guid userId, string email);
}

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
