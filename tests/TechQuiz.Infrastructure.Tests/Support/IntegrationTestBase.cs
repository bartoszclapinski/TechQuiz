using Microsoft.EntityFrameworkCore;
using TechQuiz.Infrastructure.Persistence;

namespace TechQuiz.Infrastructure.Tests.Support;

/// <summary>
/// Base class for integration tests that need a real Postgres-backed <see cref="AppDbContext"/>.
/// Truncates all data tables before each test so tests start with an empty DB without paying
/// the cost of container restart or full schema rebuild.
/// </summary>
public abstract class IntegrationTestBase(PostgresContainerFixture fixture) : IAsyncLifetime
{
    // TRUNCATE CASCADE ignores per-FK ON DELETE behaviors (Restrict is set on
    // Category→Question and QuizAttempt→Quiz to protect runtime data, but tests
    // legitimately need a full wipe). __EFMigrationsHistory is intentionally
    // excluded — schema stays, only data is cleared.
    private const string TruncateAllTables = """
        TRUNCATE TABLE
          options,
          questions,
          quiz_questions,
          quizzes,
          categories,
          quiz_attempts,
          "AspNetUserRoles",
          "AspNetUserClaims",
          "AspNetUserLogins",
          "AspNetUserTokens",
          "AspNetRoleClaims",
          "AspNetUsers",
          "AspNetRoles"
        RESTART IDENTITY CASCADE;
        """;

    protected PostgresContainerFixture Fixture { get; } = fixture;

    public async Task InitializeAsync()
    {
        await using var db = Fixture.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync(TruncateAllTables);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected AppDbContext CreateDbContext() => Fixture.CreateDbContext();
}
