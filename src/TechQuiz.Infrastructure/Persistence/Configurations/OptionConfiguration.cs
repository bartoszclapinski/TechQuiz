using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.QuestionId).IsRequired();
        builder.Property(o => o.Text).IsRequired().HasMaxLength(500);
        builder.Property(o => o.IsCorrect).IsRequired();
        builder.Property(o => o.OrderIndex).IsRequired();

        // FK back to Question is declared on the Question side (HasMany(q => q.Options))
        // so no duplicate HasOne here.

        builder.HasIndex(o => new { o.QuestionId, o.OrderIndex }).IsUnique();
    }
}
