using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TechQuiz.Application;
using TechQuiz.Infrastructure;
using TechQuiz.Infrastructure.Persistence;
using TechQuiz.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// ─── Logging ─────────────────────────────────────────────────────────────────

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// ─── Services ────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

// ─── Pipeline ────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Dev-only seeding. The seeder itself is idempotent per-resource, but the Development
// guard makes the intent explicit and keeps staging/prod hosts from running the demo
// bootstrap. Critical-level log on failure surfaces what stage of startup failed —
// re-throw preserves the existing host-aborts-on-error behaviour.
if (app.Environment.IsDevelopment())
{
    // ValidateOnStart normally fires inside IHost.StartAsync (i.e. during app.Run()).
    // The seeder runs before app.Run(), so without an explicit Validate() here a
    // misconfigured policy (e.g. Identity:Password:RequiredLength=0) would let the
    // seeder create the demo user before validation aborts the host.
    app.Services.GetRequiredService<IStartupValidator>().Validate();

    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Data seeding failed — aborting startup");
        throw;
    }
}

app.Run();
