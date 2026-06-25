namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Stores each user's own AI provider API keys, encrypted at rest (ADR-006).
/// Keys are decrypted only at use time and are never logged or returned to the
/// client. One key per (user, provider) pair; <see cref="UpsertAsync"/> rotates.
/// </summary>
public interface IAiKeyStore
{
    Task UpsertAsync(
        Guid userId, AiProviderKind kind, string apiKey, CancellationToken cancellationToken = default);

    /// <summary>Returns the decrypted key, or <c>null</c> if the user has none for this kind.</summary>
    Task<string?> GetAsync(
        Guid userId, AiProviderKind kind, CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid userId, AiProviderKind kind, CancellationToken cancellationToken = default);

    /// <summary>The provider kinds the user has a key for — kinds only, never the key material.</summary>
    Task<IReadOnlyList<AiProviderKind>> ListConfiguredAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
