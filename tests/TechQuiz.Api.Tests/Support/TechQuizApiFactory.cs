using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TechQuiz.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TechQuiz.Api.Tests.Support;

/// <summary>
/// Boots the real API host (the actual <c>Program</c> pipeline) against an ephemeral
/// Postgres container. The container is migrated before the host starts so the
/// Development-environment seeder — which runs during startup and assumes the schema
/// exists — populates the demo user and the Unit Testing quiz that the E2E tests log into.
/// </summary>
/// <remarks>
/// Overrides are pushed through process environment variables, not
/// <c>ConfigureAppConfiguration</c>: under the minimal-hosting model
/// <see cref="WebApplicationFactory{TEntryPoint}"/> drives the host through a
/// <c>DeferredHostBuilder</c>, where config added on the web-host builder is re-overridden by
/// the app's own <c>appsettings.json</c>. Environment variables sit after appsettings and dev
/// user-secrets in <c>WebApplication.CreateBuilder</c>'s provider chain, so they win in CI
/// (where the JWT signing key secret is absent) and locally alike.
/// </remarks>
public sealed class TechQuizApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // 32+ bytes — HMAC-SHA256 signing keys must be at least 256 bits or JWT bearer rejects them.
    private const string TestSigningKey = "techquiz-integration-test-signing-key-0123456789";
    private const string ConnectionStringVar = "ConnectionStrings__DefaultConnection";
    private const string SigningKeyVar = "Jwt__SigningKey";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("techquiz_api_test")
        .WithUsername("techquiz")
        .WithPassword("techquiz_test")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // Point the host (built lazily on the first CreateClient) at the container, and supply a
        // throwaway signing key so JWT validation passes without the dev user-secret.
        Environment.SetEnvironmentVariable(ConnectionStringVar, _database.GetConnectionString());
        Environment.SetEnvironmentVariable(SigningKeyVar, TestSigningKey);

        // The startup seeder does not migrate — apply the schema first via a standalone context
        // so seeding lands on real tables.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_database.GetConnectionString())
            .UseTechQuizConventions()
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development so appsettings.Development.json loads (relaxed password policy matching the
        // seeded demo user) and the startup seeder runs against the migrated container.
        builder.UseEnvironment(Environments.Development);
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable(ConnectionStringVar, null);
        Environment.SetEnvironmentVariable(SigningKeyVar, null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}
