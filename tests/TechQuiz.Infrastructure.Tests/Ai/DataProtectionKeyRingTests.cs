using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Abstractions;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Ai;

/// <summary>
/// The Data Protection key ring that encrypts users' AI keys must live in the database: on Render
/// the container file system is wiped on every deploy, restart and sleep, and a key ring kept
/// there makes every stored key undecryptable afterwards. Each test builds its own host over the
/// real <c>AddInfrastructure</c>, so disposing one and building another stands in for a restart.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class DataProtectionKeyRingTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private const string CertificatePassword = "test-only-password";

    private ServiceProvider BuildHost(IDictionary<string, string?>? extra = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = Fixture.ConnectionString,
        };
        foreach (var (key, value) in extra ?? new Dictionary<string, string?>())
        {
            settings[key] = value;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        return services.BuildServiceProvider();
    }

    private static async Task UpsertAsync(ServiceProvider host, Guid userId, string apiKey)
    {
        using var scope = host.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IAiKeyStore>()
            .UpsertAsync(userId, AiProviderKind.Anthropic, apiKey);
    }

    private static async Task<string?> GetAsync(ServiceProvider host, Guid userId)
    {
        using var scope = host.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IAiKeyStore>()
            .GetAsync(userId, AiProviderKind.Anthropic);
    }

    private async Task<List<string>> StoredKeyRingXmlAsync()
    {
        await using var db = CreateDbContext();
        return await db.DataProtectionKeys.Select(k => k.Xml!).ToListAsync();
    }

    private static string CreateCertificateBase64()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=TechQuiz test key ring", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, CertificatePassword));
    }

    [Fact]
    public async Task StoredAiKey_SurvivesAHostRestart_BecauseTheKeyRingIsInTheDatabase()
    {
        var userId = await CreateUserAsync();
        await using (var before = BuildHost())
        {
            await UpsertAsync(before, userId, "sk-ant-survives");
        }

        await using var after = BuildHost();
        var key = await GetAsync(after, userId);

        key.Should().Be("sk-ant-survives");
        (await StoredKeyRingXmlAsync()).Should().NotBeEmpty();
    }

    [Fact]
    public async Task KeyRing_IsEncryptedAtRest_WhenACertificateIsConfigured()
    {
        var userId = await CreateUserAsync();
        var certificate = new Dictionary<string, string?>
        {
            ["DataProtection:CertificateBase64"] = CreateCertificateBase64(),
            ["DataProtection:CertificatePassword"] = CertificatePassword,
        };

        await using (var before = BuildHost(certificate))
        {
            await UpsertAsync(before, userId, "sk-ant-encrypted-ring");
        }

        var xml = await StoredKeyRingXmlAsync();
        xml.Should().NotBeEmpty();
        xml.Should().OnlyContain(x => x.Contains("encryptedSecret") && !x.Contains("<masterKey"));

        await using var after = BuildHost(certificate);
        (await GetAsync(after, userId)).Should().Be("sk-ant-encrypted-ring");
    }

    [Fact]
    public void RequiredCertificate_Missing_FailsAtStartup()
    {
        var act = () => BuildHost(new Dictionary<string, string?>
        {
            ["DataProtection:RequireCertificate"] = "true",
        });

        act.Should().Throw<InvalidOperationException>().WithMessage("*DataProtection:CertificateBase64*");
    }
}
