using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Infrastructure.Persistence.Ai;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

internal sealed class UserAiKeyConfiguration : IEntityTypeConfiguration<UserAiKey>
{
    public void Configure(EntityTypeBuilder<UserAiKey> builder)
    {
        // (user, provider) is the natural key — the store contract guarantees at most
        // one key per pair, and the composite PK enforces it at the database level.
        builder.HasKey(k => new { k.UserId, k.Provider });

        // Stored as text, not the underlying int, so reordering the enum can never
        // silently repoint an existing row at a different provider.
        builder.Property(k => k.Provider)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(k => k.EncryptedApiKey).IsRequired();

        // Deleting an Identity user removes their stored keys — orphaned ciphertext
        // would never be decryptable for anyone and only sits as dead data.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
