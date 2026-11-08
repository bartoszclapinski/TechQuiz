using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>Why an AI provider refused a request made with the user's own key.</summary>
public enum AiRejectionReason
{
    /// <summary>The key is wrong, revoked or lacks permission. The user must replace it.</summary>
    InvalidKey,

    /// <summary>The key works but is rate-limited or out of credit. Retrying later may help.</summary>
    QuotaOrRateLimit,
}

/// <summary>
/// Thrown by an <see cref="IAiProvider"/> when the upstream API rejects the user's key or quota.
/// Surfaced as 422 / 429 — never 401, which the SPA treats as an expired session. The message is
/// fixed text: it never carries the key or the upstream response body.
/// </summary>
public sealed class AiProviderRejectedException(AiProviderKind kind, AiRejectionReason reason)
    : Exception(reason == AiRejectionReason.InvalidKey
        ? $"{kind} rejected the API key. Check or replace it in Settings."
        : $"{kind} is rate-limiting this key or it is out of credit. Try again later.")
{
    public AiProviderKind Kind { get; } = kind;

    public AiRejectionReason Reason { get; } = reason;
}
