using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Identity;
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
        var categoryName = await db.Categories.Select(c => c.Name).SingleAsync();
        var demoUserExists = await db.Users.AnyAsync(u => u.Email == DataSeeder.DemoUserEmail);

        categoryCount.Should().Be(1);
        questionCount.Should().Be(19);
        optionCount.Should().Be(76); // 19 questions × 4 options each
        categoryName.Should().Be("Unit Testing");
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

        categoryCount.Should().Be(1);
        questionCount.Should().Be(19);
        optionCount.Should().Be(76);
        demoUserCount.Should().Be(1);
    }

    private async Task RunSeedAsync()
    {
        // Build a minimal service provider with everything DataSeeder needs.
        // Identity defaults already accept the seeded "Demo123!" password
        // (length 8, mixed case, digit, '!'-non-alpha) — no per-test override needed.
        var services = new ServiceCollection();
        services.AddLogging(); // no providers — ILogger<DataSeeder> resolves to a no-op
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(Fixture.ConnectionString).UseTechQuizConventions());
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<DataSeeder>();

        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
