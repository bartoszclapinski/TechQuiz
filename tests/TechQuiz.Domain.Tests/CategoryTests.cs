using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class CategoryTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var id = Guid.NewGuid();

        var category = new Category(id, "C# Basics", "Fundamentals of C#", "csharp");

        category.Id.Should().Be(id);
        category.Name.Should().Be("C# Basics");
        category.Description.Should().Be("Fundamentals of C#");
        category.IconCode.Should().Be("csharp");
    }
}
