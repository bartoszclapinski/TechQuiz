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

        // IUserContext reads from the request HttpContext; IHttpContextAccessor itself
        // is registered by the API host (Program.cs) since it only makes sense in an
        // HTTP composition root.
        services.AddScoped<IUserContext, HttpUserContext>();

        return services;
    }
}
