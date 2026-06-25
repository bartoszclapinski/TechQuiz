using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class PooledQuestionConfiguration : IEntityTypeConfiguration<PooledQuestion>
{
    public void Configure(EntityTypeBuilder<PooledQuestion> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.CreatedByUserId).IsRequired();
        builder.Property(q => q.Provider).IsRequired().HasMaxLength(64);
        builder.Property(q => q.Topic).IsRequired().HasMaxLength(256);
        builder.Property(q => q.GeneratedAtUtc).IsRequired();
        builder.Property(q => q.Type).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(q => q.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.Status).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.Text).IsRequired().HasMaxLength(2000);
        builder.Property(q => q.Explanation).HasMaxLength(4000);

        builder.HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey("PooledQuestionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Browse lists filter by Status; the author dashboard (later) filters by owner.
        builder.HasIndex(q => q.Status);
        builder.HasIndex(q => q.CreatedByUserId);
    }
}
