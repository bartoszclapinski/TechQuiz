using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Seed;
using Testcontainers.PostgreSql;

namespace TechQuiz.Infrastructure.Tests.Support;

/// <summary>
/// Spins one ephemeral Postgres container per test run, applies all migrations on
/// start, and exposes the shared building blocks every integration test needs:
/// a fresh <see cref="AppDbContext"/> factory, a TRUNCATE helper driven by the
/// live schema (so adding a table does not silently leak state across tests),
/// and an <see cref="IServiceProvider"/> wired with Identity + the seeder so
/// tests do not re-implement DI bootstrap.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("techquiz_test")
        .WithUsername("techquiz")
        .WithPassword("techquiz_test")
        .Build();

    private string[] _truncateTargets = [];
    private ServiceProvider? _services;

    public string ConnectionString => _container.GetConnectionString();

    public IServiceProvider Services =>
        _services ?? throw new InvalidOperationException(
            "Fixture has not been initialized — Services is only available after InitializeAsync.");

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using (var db = CreateDbContext())
        {
            await db.Database.MigrateAsync();
            _truncateTargets = await db.Database
                .SqlQueryRaw<string>("""
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name <> '__EFMigrationsHistory'
                    """)
                .ToArrayAsync();
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(ConnectionString).UseTechQuizConventions());
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<DataSeeder>();
        _services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (_services is not null)
        {
            await _services.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseTechQuizConventions()
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Clears every user-data table discovered at init time. Identity tables (PascalCase)
    /// and domain tables (snake_case) are quoted uniformly so the same statement works for
    /// both. <c>__EFMigrationsHistory</c> is excluded — the schema stays, only data is wiped.
    /// </summary>
    public async Task TruncateAllDataAsync()
    {
        if (_truncateTargets.Length == 0)
        {
            return;
        }

        var quoted = string.Join(", ", _truncateTargets.Select(t => $"\"{t}\""));
        await using var db = CreateDbContext();

        // EF1002 (raw SQL injection) is a false positive here — identifiers come from
        // information_schema.tables read once at init, never from caller input.
#pragma warning disable EF1002
        await db.Database.ExecuteSqlRawAsync(
            $"TRUNCATE TABLE {quoted} RESTART IDENTITY CASCADE;");
#pragma warning restore EF1002
    }
}
