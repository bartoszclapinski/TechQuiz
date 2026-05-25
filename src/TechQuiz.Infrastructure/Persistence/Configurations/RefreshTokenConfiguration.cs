using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Token).IsRequired().HasMaxLength(256);
        builder.Property(t => t.IssuedAt).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.RevokedAt);

        // Token is the lookup key in the refresh flow — uniqueness guarantees
        // FindByTokenAsync returns at most one row and protects against accidental
        // duplicate inserts (which the random generator should already prevent).
        builder.HasIndex(t => t.Token).IsUnique();

        // (UserId, ExpiresAt) is the natural shape for future cleanup queries
        // ("delete expired tokens for user X" or "list active sessions").
        builder.HasIndex(t => new { t.UserId, t.ExpiresAt });

        // Deleting an Identity user wipes their refresh tokens — orphan rows
        // would just rot in the table and never authenticate anyone anyway.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
