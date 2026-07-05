using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class CategoryTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var id = Guid.NewGuid();
        var trackId = Guid.NewGuid();

        var category = new Category(id, trackId, "C# Basics", "Fundamentals of C#", "csharp", position: 2);

        category.Id.Should().Be(id);
        category.TrackId.Should().Be(trackId);
        category.Name.Should().Be("C# Basics");
        category.Description.Should().Be("Fundamentals of C#");
        category.IconCode.Should().Be("csharp");
        category.Position.Should().Be(2);
    }
}
