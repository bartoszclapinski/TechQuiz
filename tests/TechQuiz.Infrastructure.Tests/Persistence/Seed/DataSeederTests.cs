using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Infrastructure.Persistence.Seed;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Persistence.Seed;

[Collection(DatabaseCollection.Name)]
public sealed class DataSeederTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SeedAsync_PopulatesCategoryQuestionsAndOptions()
    {
        await RunSeedAsync();

        await using var db = CreateDbContext();
        var trackCount = await db.Tracks.CountAsync();
        var categoryCount = await db.Categories.CountAsync();
        var questionCount = await db.Questions.CountAsync();
        var optionCount = await db.Options.CountAsync();
        var trackNames = await db.Tracks.Select(t => t.Name).ToListAsync();
        var categoryNames = await db.Categories.Select(c => c.Name).ToListAsync();
        var demoUserExists = await db.Users.AnyAsync(u => u.Email == DataSeeder.DemoUserEmail);

        trackCount.Should().Be(4);
        categoryCount.Should().Be(18);
        // Same 269 questions as the flat catalogue — the taxonomy repartitions them, it does not add or remove any.
        questionCount.Should().Be(269);
        optionCount.Should().Be(1076); // 269 questions × 4 options each
        trackNames.Should().BeEquivalentTo([".NET", "Databases", "Front-End", "Engineering Practices"]);
        categoryNames.Should().BeEquivalentTo([
            "C#/.NET", "ASP.NET Core", "EF Core", "ADO.NET", "Unit Testing", "Design Patterns",
            "Database Fundamentals", "Normalization", "Querying", "Data Manipulation", "Schema Definition",
            "JavaScript", "Async & Events", "TypeScript", "HTML & CSS",
            "Git & Version Control", "CI/CD", "Clean Code",
        ]);
        demoUserExists.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentPerResource_WhenInvokedTwice()
    {
        await RunSeedAsync();
        await RunSeedAsync(); // second call must be a no-op per resource

        await using var db = CreateDbContext();
        var trackCount = await db.Tracks.CountAsync();
        var categoryCount = await db.Categories.CountAsync();
        var questionCount = await db.Questions.CountAsync();
        var optionCount = await db.Options.CountAsync();
        var demoUserCount = await db.Users.CountAsync(u => u.Email == DataSeeder.DemoUserEmail);

        trackCount.Should().Be(4);
        categoryCount.Should().Be(18);
        questionCount.Should().Be(269);
        optionCount.Should().Be(1076);
        demoUserCount.Should().Be(1);
    }

    private async Task RunSeedAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
