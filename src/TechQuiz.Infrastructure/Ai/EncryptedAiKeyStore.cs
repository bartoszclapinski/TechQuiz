using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Ai;

namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Stores each user's provider keys encrypted at rest via ASP.NET Data Protection
/// (ADR-006). Plaintext exists only transiently inside <see cref="UpsertAsync"/> and
/// <see cref="GetAsync"/>; it is never persisted, logged, or returned by the listing
/// methods.
/// </summary>
internal sealed class EncryptedAiKeyStore(AppDbContext db, IDataProtectionProvider protection)
    : IAiKeyStore
{
    // Versioned purpose string: bumping it (…v2) would intentionally invalidate every
    // existing ciphertext, e.g. if the encoding ever changes.
    private readonly IDataProtector _protector = protection.CreateProtector("TechQuiz.AiKeys.v1");

    public async Task UpsertAsync(
        Guid userId, AiProviderKind kind, string apiKey, CancellationToken cancellationToken = default)
    {
        var ciphertext = _protector.Protect(apiKey);

        var existing = await db.Set<UserAiKey>()
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Provider == kind, cancellationToken);

        if (existing is null)
        {
            db.Add(new UserAiKey(userId, kind, ciphertext));
        }
        else
        {
            existing.Rotate(ciphertext);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetAsync(
        Guid userId, AiProviderKind kind, CancellationToken cancellationToken = default)
    {
        var row = await db.Set<UserAiKey>()
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.UserId == userId && k.Provider == kind, cancellationToken);

        return row is null ? null : _protector.Unprotect(row.EncryptedApiKey);
    }

    public Task RemoveAsync(
        Guid userId, AiProviderKind kind, CancellationToken cancellationToken = default) =>
        db.Set<UserAiKey>()
            .Where(k => k.UserId == userId && k.Provider == kind)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<IReadOnlyList<AiProviderKind>> ListConfiguredAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await db.Set<UserAiKey>()
            .AsNoTracking()
            .Where(k => k.UserId == userId)
            .Select(k => k.Provider)
            .ToListAsync(cancellationToken);
}
