using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.QuizId).IsRequired();
        builder.Property(a => a.StartedAt).IsRequired();
        builder.Property(a => a.CompletedAt);

        // CompletedAt drives IsCompleted — don't persist the derived property.
        builder.Ignore(a => a.IsCompleted);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        // Answers are an owned collection of QuizAttempt — no independent identity in the Domain.
        // EF reads/writes them via the private backing field "_answers" exposed by IReadOnlyList<Answer> Answers.
        builder.OwnsMany<Answer>(nameof(QuizAttempt.Answers), owned =>
        {
            owned.WithOwner().HasForeignKey("QuizAttemptId");
            owned.HasKey("QuizAttemptId", nameof(Answer.QuestionId));
            owned.Property(a => a.QuestionId).ValueGeneratedNever().IsRequired();
            owned.Property(a => a.SelectedOptionId);
            owned.Property(a => a.SubmittedAt).IsRequired();
        });

        builder.Navigation(a => a.Answers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => new { a.UserId, a.StartedAt });
    }
}
