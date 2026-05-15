using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechQuiz.Infrastructure.Persistence;

/// <summary>
/// Enables <c>dotnet ef</c> commands (migrations, database update, scaffolding) to construct
/// an <see cref="AppDbContext"/> without booting the full API. Resolves the connection string
/// in this order:
/// <list type="number">
///   <item>environment variable <c>DOTNET_EF_CONNECTION_STRING</c> — per-machine override</item>
///   <item>hard-coded local-dev default matching <c>docker-compose.yml</c></item>
/// </list>
/// Production migrations should run via the API host (real configuration), not this factory.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string LocalDevConnectionString =
        "Host=localhost;Port=5433;Database=techquiz;Username=techquiz;Password=techquiz_dev";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DOTNET_EF_CONNECTION_STRING")
            ?? LocalDevConnectionString;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseTechQuizConventions()
            .Options;

        return new AppDbContext(options);
    }
}
