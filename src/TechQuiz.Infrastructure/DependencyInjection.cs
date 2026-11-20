using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
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
        // production-safe defaults in appsettings.json (RequiredLength 12) can be relaxed
        // per-environment; the dev override lives in appsettings.Development.json. The seeded
        // demo password (DemoPass123!) satisfies the strict 12-char default, so seeding works
        // under Staging's production-safe policy too.
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
        services.AddScoped<IPooledQuestionRepository, PooledQuestionRepository>();
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

        // AI providers (ADR-006, ADR-019). The resolver picks a provider by Kind from the
        // registered set; kinds without a registered provider resolve to
        // UnknownAiProviderException.
        services
            .AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                "Ai:Anthropic:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        // Typed HttpClient → transient. The resolver is scoped (not singleton) so it never
        // captures the typed client, keeping HttpClient handler rotation intact. Each provider's
        // client is named: unnamed AddHttpClient<IAiProvider, …> calls share one name, so the
        // last BaseAddress would win for every provider (guarded by AiProviderWiringTests).
        services.AddHttpClient<IAiProvider, AnthropicAiProvider>("Anthropic", (provider, client) =>
        {
            var anthropic = provider.GetRequiredService<IOptions<AnthropicOptions>>().Value;
            client.BaseAddress = new Uri(anthropic.BaseUrl.TrimEnd('/') + "/");
        });

        services
            .AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                "Ai:OpenAi:BaseUrl must be an absolute URL.")
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.Model) && o.MaxCompletionTokens > 0,
                "Ai:OpenAi:Model must be set and Ai:OpenAi:MaxCompletionTokens must be positive.")
            .ValidateOnStart();

        services.AddHttpClient<IAiProvider, OpenAiAiProvider>("OpenAi", (provider, client) =>
        {
            var openAi = provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(openAi.BaseUrl.TrimEnd('/') + "/");
        });

        services.AddScoped<IAiProviderResolver, AiProviderResolver>();

        // Bring-your-own-key storage (ADR-006): user keys are encrypted at rest with
        // ASP.NET Data Protection. Scoped because the store depends on AppDbContext.
        AddKeyRing(services, configuration);
        services.AddScoped<IAiKeyStore, EncryptedAiKeyStore>();

        return services;
    }

    /// <summary>
    /// Stores the Data Protection key ring in the database (ADR-028): the default file-system ring
    /// is wiped with the container on Render, which makes every stored AI key undecryptable. When
    /// a certificate is configured the ring is encrypted with it, so the database alone cannot
    /// decrypt users' keys. A fixed application name keeps payloads readable across hosts and
    /// container paths.
    /// </summary>
    private static void AddKeyRing(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(KeyRingOptions.SectionName).Get<KeyRingOptions>()
            ?? new KeyRingOptions();

        var dataProtection = services.AddDataProtection()
            .SetApplicationName("TechQuiz")
            .PersistKeysToDbContext<AppDbContext>();

        if (string.IsNullOrWhiteSpace(options.CertificateBase64))
        {
            if (options.RequireCertificate)
            {
                throw new InvalidOperationException(
                    "DataProtection:CertificateBase64 is required in this environment: without it the key " +
                    "ring that encrypts users' AI keys would be stored unencrypted in the database.");
            }

            return;
        }

        var certificate = X509CertificateLoader.LoadPkcs12(
            Convert.FromBase64String(options.CertificateBase64),
            options.CertificatePassword,
            X509KeyStorageFlags.EphemeralKeySet);
        // Decryption must be given the certificate too: by default it is looked up in the OS
        // certificate store, which is empty in the container.
        dataProtection
            .ProtectKeysWithCertificate(certificate)
            .UnprotectKeysWithAnyCertificate(certificate);
    }
}
