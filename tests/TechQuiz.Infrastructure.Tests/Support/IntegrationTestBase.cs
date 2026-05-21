using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Tests.Support;

/// <summary>
/// Base class for integration tests that need a real Postgres-backed <see cref="AppDbContext"/>.
/// Truncates all data tables before each test (schema is discovered dynamically by the fixture)
/// and exposes helpers backed by the shared <see cref="PostgresContainerFixture.Services"/>
/// provider so tests do not re-build Identity/seeder DI per call.
/// </summary>
public abstract class IntegrationTestBase(PostgresContainerFixture fixture) : IAsyncLifetime
{
    protected PostgresContainerFixture Fixture { get; } = fixture;

    public Task InitializeAsync() => Fixture.TruncateAllDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected AppDbContext CreateDbContext() => Fixture.CreateDbContext();

    /// <summary>
    /// Creates an Identity user via <see cref="UserManager{TUser}"/> — same code path the
    /// API uses at sign-up, so security stamps, normalised identifiers, and any future
    /// required Identity field land correctly without per-test plumbing.
    /// </summary>
    protected async Task<Guid> CreateUserAsync(string? email = null)
    {
        var userId = Guid.NewGuid();
        email ??= $"user-{userId:N}@test.local";

        using var scope = Fixture.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, "TestPassword123!");
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        return userId;
    }
}
