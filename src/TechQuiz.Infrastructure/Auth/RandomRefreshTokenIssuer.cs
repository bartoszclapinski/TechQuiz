using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Auth;

public sealed class RandomRefreshTokenIssuer(IOptions<JwtOptions> options) : IRefreshTokenIssuer
{
    private const int TokenByteLength = 32; // 256 bits

    private readonly TimeSpan _lifetime = TimeSpan.FromDays(options.Value.RefreshTokenLifetimeDays);

    public IssuedRefreshToken Issue(Guid userId, DateTimeOffset now)
    {
        var rawToken = GenerateOpaqueToken();
        var entity = RefreshToken.Issue(
            Guid.NewGuid(), userId, RefreshTokenHasher.Hash(rawToken), now, _lifetime);
        return new IssuedRefreshToken(entity, rawToken);
    }

    private static string GenerateOpaqueToken()
    {
        Span<byte> buffer = stackalloc byte[TokenByteLength];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer);
    }
}
