using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class ReviewSessionConfiguration : IEntityTypeConfiguration<ReviewSession>
{
    public void Configure(EntityTypeBuilder<ReviewSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.CompletedAt).IsRequired();

        // Derived from the owned items — don't persist a redundant count column.
        builder.Ignore(s => s.QuestionCount);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Items are an owned collection of ReviewSession — no independent identity in the Domain.
        // Same audit-trail stance as QuizAttempt.Answers: QuestionId / SelectedOptionId are stored as
        // plain Guids with NO enforced FK back to questions / options, so a session survives a question
        // being edited or removed later.
        builder.OwnsMany<ReviewItem>(nameof(ReviewSession.Items), owned =>
        {
            owned.ToTable("review_items");
            owned.WithOwner().HasForeignKey("ReviewSessionId");
            owned.HasKey("ReviewSessionId", nameof(ReviewItem.QuestionId));
            owned.Property(i => i.QuestionId).ValueGeneratedNever().IsRequired();
            owned.Property(i => i.SelectedOptionId);
        });

        builder.Navigation(s => s.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => new { s.UserId, s.CompletedAt });
    }
}
