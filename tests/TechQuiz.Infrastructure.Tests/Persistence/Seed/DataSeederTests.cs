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
        var categoryCount = await db.Categories.CountAsync();
        var questionCount = await db.Questions.CountAsync();
        var optionCount = await db.Options.CountAsync();
        var categoryNames = await db.Categories.Select(c => c.Name).ToListAsync();
        var demoUserExists = await db.Users.AnyAsync(u => u.Email == DataSeeder.DemoUserEmail);

        categoryCount.Should().Be(3);
        questionCount.Should().Be(59); // 19 Unit Testing + 20 SQL + 20 EF Core
        optionCount.Should().Be(236); // 59 questions × 4 options each
        categoryNames.Should().BeEquivalentTo(["Unit Testing", "SQL", "EF Core"]);
        demoUserExists.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_IsIdempotentPerResource_WhenInvokedTwice()
    {
        await RunSeedAsync();
        await RunSeedAsync(); // second call must be a no-op per resource

        await using var db = CreateDbContext();
        var categoryCount = await db.Categories.CountAsync();
        var questionCount = await db.Questions.CountAsync();
        var optionCount = await db.Options.CountAsync();
        var demoUserCount = await db.Users.CountAsync(u => u.Email == DataSeeder.DemoUserEmail);

        categoryCount.Should().Be(3);
        questionCount.Should().Be(59);
        optionCount.Should().Be(236);
        demoUserCount.Should().Be(1);
    }

    private async Task RunSeedAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
