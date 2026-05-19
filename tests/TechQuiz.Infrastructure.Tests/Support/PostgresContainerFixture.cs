using Microsoft.EntityFrameworkCore;
using TechQuiz.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TechQuiz.Infrastructure.Tests.Support;

/// <summary>
/// Spins one ephemeral Postgres container per test run, applies all migrations on
/// start, and exposes <see cref="CreateDbContext"/> for tests that need a real
/// <see cref="AppDbContext"/>. Lifecycle is managed by xUnit via <see cref="DatabaseCollection"/>.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("techquiz_test")
        .WithUsername("techquiz")
        .WithPassword("techquiz_test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseTechQuizConventions()
            .Options;

        return new AppDbContext(options);
    }
}
