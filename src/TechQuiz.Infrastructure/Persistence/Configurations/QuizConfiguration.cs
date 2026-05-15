using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.CategoryId).IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many Quiz <-> Question via implicit join table (quiz_questions).
        // Question side has no inverse navigation (a Question doesn't need to know which Quizzes use it).
        builder.HasMany(q => q.Questions)
            .WithMany();

        builder.HasIndex(q => q.CategoryId);
    }
}
