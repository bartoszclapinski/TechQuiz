using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Infrastructure.Ai;
using TechQuiz.Infrastructure.Auth;
using TechQuiz.Infrastructure.CodeExecution;
using TechQuiz.Infrastructure.Identity;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Repositories;
using TechQuiz.Infrastructure.Persistence.Seed;

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

        // AddIdentityCore (rather than AddIdentity) so we get UserManager / RoleManager
        // without the cookie auth scheme — JWT bearer is the project's auth scheme and
        // would conflict with Identity's default cookie wiring.
        //
        // Password policy is bound from the Identity:Password configuration section so
        // production-safe defaults in appsettings.json can be relaxed per-environment.
        // The dev override lives in appsettings.Development.json, matching the seeded
        // demo password (Demo123!).
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                configuration.GetSection("Identity:Password").Bind(options.Password);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Defensive floor: refuse to start if password policy is misconfigured below
        // the NIST-aligned minimum. Without this, a deploy slot accidentally setting
        // Identity__Password__RequiredLength=0 would silently weaken auth.
        services
            .AddOptions<IdentityOptions>()
            .Validate(
                o => o.Password.RequiredLength >= 8,
                "Identity:Password:RequiredLength must be at least 8.")
            .ValidateOnStart();

        // Repositories + unit-of-work are scoped because they depend on AppDbContext
        // (scoped per request).
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DataSeeder>();

        // IUserContext reads from the request HttpContext; IHttpContextAccessor itself
        // is registered by the API host (Program.cs) since it only makes sense in an
        // HTTP composition root.
        services.AddScoped<IUserContext, HttpUserContext>();

        // Strongly-typed Jwt:* binding. SigningKey must come from a secret store in
        // staging/prod (env var, KeyVault); appsettings.json carries only public values.
        // ValidateOnStart prevents booting with an empty/missing signing key.
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.SigningKey),
                "Jwt:SigningKey must be configured (env var or dotnet user-secrets).")
            .Validate(
                o => o.AccessTokenLifetimeMinutes > 0,
                "Jwt:AccessTokenLifetimeMinutes must be positive.")
            .Validate(
                o => o.RefreshTokenLifetimeDays > 0,
                "Jwt:RefreshTokenLifetimeDays must be positive.")
            .ValidateOnStart();

        // Auth services. Singleton for the stateless ones (JWT signing, RNG); scoped for
        // IdentityUserAccountService since UserManager itself is scoped.
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenIssuer, RandomRefreshTokenIssuer>();
        services.AddScoped<IUserAccountService, IdentityUserAccountService>();

        // Code execution via self-hosted Judge0 (ADR-018). BaseUrl must be set so the
        // typed client has a target; ValidateOnStart fails fast on a missing endpoint.
        services
            .AddOptions<Judge0Options>()
            .Bind(configuration.GetSection(Judge0Options.SectionName))
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                "Judge0:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.AddHttpClient<ICodeExecutor, Judge0CodeExecutor>((provider, client) =>
        {
            var judge0 = provider.GetRequiredService<IOptions<Judge0Options>>().Value;
            client.BaseAddress = new Uri(judge0.BaseUrl.TrimEnd('/') + "/");
        });

        // In-memory seed of coding challenges (ADR-018); singleton since the seed is immutable.
        services.AddSingleton<ICodeChallengeCatalog, InMemoryCodeChallengeCatalog>();

        // AI question generation (ADR-006). The resolver picks a provider by Kind from
        // the registered set; StubAiProvider is the iteration-3.1 placeholder for the
        // Anthropic kind and is replaced by a real client in 3.2. Stateless → singleton.
        services.AddSingleton<IAiProvider, StubAiProvider>();
        services.AddSingleton<IAiProviderResolver, AiProviderResolver>();

        return services;
    }
}
