using TechQuiz.Application.Abstractions;

namespace TechQuiz.Infrastructure.Persistence.Ai;

/// <summary>
/// Persistence row for one user's API key for a single provider. The key material is
/// stored already-encrypted (<see cref="EncryptedApiKey"/>) — this type never holds
/// plaintext. One row per (user, provider); rotation replaces the ciphertext in place.
/// </summary>
internal sealed class UserAiKey
{
    public Guid UserId { get; private set; }
    public AiProviderKind Provider { get; private set; }
    public string EncryptedApiKey { get; private set; } = string.Empty;

    private UserAiKey()
    {
    }

    public UserAiKey(Guid userId, AiProviderKind provider, string encryptedApiKey)
    {
        UserId = userId;
        Provider = provider;
        EncryptedApiKey = encryptedApiKey;
    }

    public void Rotate(string encryptedApiKey) => EncryptedApiKey = encryptedApiKey;
}
