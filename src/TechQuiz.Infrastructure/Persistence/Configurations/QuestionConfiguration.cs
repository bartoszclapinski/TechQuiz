using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.CategoryId).IsRequired();
        builder.Property(q => q.Type).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(q => q.Difficulty).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(q => q.Text).IsRequired().HasMaxLength(2000);
        builder.Property(q => q.Explanation).IsRequired().HasMaxLength(4000);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.CategoryId);
        builder.HasIndex(q => new { q.CategoryId, q.Difficulty });
    }
}
