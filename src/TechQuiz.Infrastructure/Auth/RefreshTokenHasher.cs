using System.Security.Cryptography;
using System.Text;

namespace TechQuiz.Infrastructure.Auth;

/// <summary>
/// Hashes the opaque refresh-token secret for storage. The raw value is high-entropy
/// (256 random bits), so a plain SHA-256 is sufficient — no salt or work factor is needed
/// the way it would be for low-entropy passwords. Storing the hash means a database leak
/// yields hashes, not usable tokens.
/// </summary>
internal static class RefreshTokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
