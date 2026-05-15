using Microsoft.EntityFrameworkCore;

namespace TechQuiz.Infrastructure.Persistence;

/// <summary>
/// Bundles TechQuiz-wide EF Core conventions so the runtime DI registration and
/// the design-time tooling factory stay in lockstep — add a new convention here
/// and every call site picks it up.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseTechQuizConventions(this DbContextOptionsBuilder builder) =>
        builder.UseSnakeCaseNamingConvention();

    public static DbContextOptionsBuilder<TContext> UseTechQuizConventions<TContext>(
        this DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext =>
        builder.UseSnakeCaseNamingConvention();
}
