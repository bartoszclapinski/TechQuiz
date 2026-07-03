using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using TechQuiz.Api.ErrorHandling;
using TechQuiz.Api.OpenApi;
using TechQuiz.Application;
using TechQuiz.Infrastructure;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Container hosts (Render and similar PaaS) inject the port to listen on via PORT and route traffic
// there; honour it so the app binds where the platform expects (ADR-022). Locally PORT is unset and
// the Dockerfile's ASPNETCORE_URLS default (or launch settings) stands.
var listenPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(listenPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{listenPort}");
}

// ─── Logging ─────────────────────────────────────────────────────────────────

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// ─── Services ────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// Global exception → ProblemDetails (RFC 7807). AddProblemDetails wires the
// IProblemDetailsService that GlobalExceptionHandler writes through; without it
// every error path returns a bare 500 (or the dev HTML error page in Development).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// IHttpContextAccessor exposes the ambient HttpContext to scoped services (consumed by
// HttpUserContext in Infrastructure). Registered here because it's an HTTP-host concern
// — keeps Infrastructure portable to non-HTTP composition roots.
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "postgres", tags: ["db", "ready"]);

// JWT bearer authentication. Signing key is read from configuration —
// in dev it comes from `dotnet user-secrets` (key: "Jwt:SigningKey"),
// in staging/prod from environment variables.
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["SigningKey"]
    ?? throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it via `dotnet user-secrets set \"Jwt:SigningKey\" \"<base64-key>\"` for dev, or environment variable for staging/prod.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// CORS for the web frontend. AllowCredentials is required because the refresh token rides in an
// HttpOnly cookie (memory-only JWT + cookie refresh — see CLAUDE.md), and that rules out
// AllowAnyOrigin, so origins are listed explicitly. The allowed origins come from configuration
// (Cors:AllowedOrigins) so the deployed web origin is supplied per-environment (ADR-022) rather than
// hardcoded; absent config (local dev) falls back to the Vite dev origin.
const string webCorsPolicy = "web";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:5173"];
}
builder.Services.AddCors(options =>
{
    options.AddPolicy(webCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ─── Pipeline ────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseSerilogRequestLogging();

// Catches exceptions from all downstream middleware (incl. controllers + MediatR) and
// renders ProblemDetails via GlobalExceptionHandler. Registered before routing so it
// wraps the whole request pipeline; sits inside Serilog logging so the mapped status is logged.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors(webCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Seeding runs for Development and Staging. The seeder is idempotent per-resource (a no-op on a
// non-empty DB), so seeding the staging host makes the live portfolio URL instantly demo-able (demo
// user + questions) without manual data entry (ADR-022). A future Production tier stays unseeded —
// it is deliberately excluded from this guard. Critical-level log on failure surfaces what stage of
// startup failed; re-throw preserves the existing host-aborts-on-error behaviour.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    // ValidateOnStart normally fires inside IHost.StartAsync (i.e. during app.Run()).
    // The seeder runs before app.Run(), so without an explicit Validate() here a
    // misconfigured policy (e.g. Identity:Password:RequiredLength=0) would let the
    // seeder create the demo user before validation aborts the host.
    app.Services.GetRequiredService<IStartupValidator>().Validate();

    try
    {
        using var scope = app.Services.CreateScope();

        // Bring the schema up to date before seeding. On a fresh managed database (e.g. Neon) no
        // migrations have been applied yet, so the seeder's first query would hit a missing table.
        // MigrateAsync is idempotent — it applies only pending migrations and is a no-op once the
        // schema is current (ADR-022). A future Production tier would migrate here too but stays
        // unseeded via the guard above.
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration or seeding failed — aborting startup");
        throw;
    }
}

app.Run();

// Exposes the implicit Program class so WebApplicationFactory<Program> in the API test
// project can boot the real host. Top-level statements otherwise compile Program as internal.
public partial class Program;
