using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class PooledQuestionOptionConfiguration : IEntityTypeConfiguration<PooledQuestionOption>
{
    public void Configure(EntityTypeBuilder<PooledQuestionOption> builder)
    {
        builder.ToTable("pooled_question_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text).IsRequired().HasMaxLength(500);
        builder.Property(o => o.IsCorrect).IsRequired();
        builder.Property(o => o.OrderIndex).IsRequired();

        // FK back to PooledQuestion is the shadow property created by the parent's
        // HasMany(q => q.Options).WithOne(); the option carries no parent id of its own.
    }
}
