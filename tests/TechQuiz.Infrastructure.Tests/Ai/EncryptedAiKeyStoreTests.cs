using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TechQuiz.Application.Abstractions;
using TechQuiz.Infrastructure.Ai;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Ai;

[Collection(DatabaseCollection.Name)]
public sealed class EncryptedAiKeyStoreTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    // One shared provider per test so ciphertext written by one store instance can be
    // read back by another — mirrors a stable host key ring across scoped requests.
    private readonly IDataProtectionProvider _protection = new EphemeralDataProtectionProvider();

    private EncryptedAiKeyStore CreateStore() => new(CreateDbContext(), _protection);

    [Fact]
    public async Task Upsert_ThenGet_RoundTripsPlaintext()
    {
        var userId = await CreateUserAsync();
        await CreateStore().UpsertAsync(userId, AiProviderKind.Anthropic, "sk-ant-secret");

        var key = await CreateStore().GetAsync(userId, AiProviderKind.Anthropic);

        key.Should().Be("sk-ant-secret");
    }

    [Fact]
    public async Task Upsert_PersistsCiphertext_NotPlaintext()
    {
        var userId = await CreateUserAsync();
        await CreateStore().UpsertAsync(userId, AiProviderKind.Anthropic, "sk-ant-secret");

        await using var db = CreateDbContext();
        var stored = await db.Database
            .SqlQueryRaw<string>("SELECT encrypted_api_key AS \"Value\" FROM user_ai_key")
            .SingleAsync();

        stored.Should().NotBe("sk-ant-secret");
        stored.Should().NotContain("sk-ant-secret");
    }

    [Fact]
    public async Task Upsert_Twice_RotatesKeyInPlace()
    {
        var userId = await CreateUserAsync();
        await CreateStore().UpsertAsync(userId, AiProviderKind.Anthropic, "first");
        await CreateStore().UpsertAsync(userId, AiProviderKind.Anthropic, "second");

        var key = await CreateStore().GetAsync(userId, AiProviderKind.Anthropic);
        key.Should().Be("second");

        await using var db = CreateDbContext();
        var rowCount = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*)::int AS \"Value\" FROM user_ai_key")
            .SingleAsync();
        rowCount.Should().Be(1);
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenNoKey()
    {
        var userId = await CreateUserAsync();

        var key = await CreateStore().GetAsync(userId, AiProviderKind.Anthropic);

        key.Should().BeNull();
    }

    [Fact]
    public async Task ListConfigured_ReturnsKindsForUser_ScopedAndKeyless()
    {
        var user = await CreateUserAsync();
        var other = await CreateUserAsync();
        await CreateStore().UpsertAsync(user, AiProviderKind.Anthropic, "a");
        await CreateStore().UpsertAsync(user, AiProviderKind.OpenAi, "b");
        await CreateStore().UpsertAsync(other, AiProviderKind.Gemini, "c");

        var kinds = await CreateStore().ListConfiguredAsync(user);

        kinds.Should().BeEquivalentTo([AiProviderKind.Anthropic, AiProviderKind.OpenAi]);
    }

    [Fact]
    public async Task Remove_DeletesOnlyThatProvidersKey()
    {
        var user = await CreateUserAsync();
        await CreateStore().UpsertAsync(user, AiProviderKind.Anthropic, "a");
        await CreateStore().UpsertAsync(user, AiProviderKind.OpenAi, "b");

        await CreateStore().RemoveAsync(user, AiProviderKind.Anthropic);

        var kinds = await CreateStore().ListConfiguredAsync(user);
        kinds.Should().BeEquivalentTo([AiProviderKind.OpenAi]);
    }
}
