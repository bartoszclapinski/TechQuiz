namespace TechQuiz.Infrastructure.Auth;

/// <summary>
/// Strongly-typed binding for the <c>Jwt:*</c> configuration section. <see cref="SigningKey"/>
/// must come from a secret store in production — appsettings.json carries only public values
/// (issuer/audience/lifetimes). Validated on startup in <c>AddInfrastructure</c>.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 14;
}
