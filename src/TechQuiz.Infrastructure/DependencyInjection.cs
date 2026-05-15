using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Abstractions;
using TechQuiz.Infrastructure.Identity;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Repositories;

namespace TechQuiz.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseTechQuizConventions());

        // Repositories + unit-of-work are scoped because they depend on AppDbContext
        // (scoped per request).
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // IUserContext reads from the request HttpContext; HttpContextAccessor exposes
        // the ambient async-local and is registered as a singleton by convention.
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();

        // TimeProvider is stateless from a consumer's perspective — share one instance.
        // Application handlers were authored against this abstraction in iteration 1.2.
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
