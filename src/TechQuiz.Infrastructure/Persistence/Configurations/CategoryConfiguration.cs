using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.Property(c => c.IconCode).IsRequired().HasMaxLength(50);
        builder.Property(c => c.TrackId).IsRequired();
        builder.Property(c => c.Position);

        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.TrackId);

        // A category belongs to exactly one track; deleting a track cascades to its categories
        // (and onward to quizzes/questions). No navigation properties on either side — the domain
        // keeps entities flat and joins happen in query projections.
        builder.HasOne<Track>()
            .WithMany()
            .HasForeignKey(c => c.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
