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

        // Many-to-many Quiz <-> Question. Question side has no inverse navigation
        // (a Question doesn't need to know which Quizzes use it). The join table name
        // is pinned to `quiz_questions` — EF Core's default would be `question_quiz`
        // (alphabetical + singular).
        builder.HasMany(q => q.Questions)
            .WithMany()
            .UsingEntity("quiz_questions");

        builder.Navigation(q => q.Questions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(q => q.CategoryId);
    }
}
