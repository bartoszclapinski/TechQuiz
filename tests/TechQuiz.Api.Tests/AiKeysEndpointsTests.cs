using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AiKeysEndpointsTests(TechQuizApiFactory factory)
{
    private sealed record SetKeyBody(string Provider, string ApiKey);

    [Fact]
    public async Task Get_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/ai/keys");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Set_then_list_then_remove_round_trips_for_the_user()
    {
        var client = await factory.CreateDemoClientAsync();

        var set = await client.PutAsJsonAsync(
            "/api/ai/keys", new SetKeyBody("Anthropic", "sk-ant-roundtrip"));
        set.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listed = await client.GetFromJsonAsync<string[]>("/api/ai/keys");
        listed.Should().Contain("Anthropic");

        var removed = await client.DeleteAsync("/api/ai/keys/Anthropic");
        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterRemoval = await client.GetFromJsonAsync<string[]>("/api/ai/keys");
        afterRemoval.Should().NotContain("Anthropic");
    }

    [Fact]
    public async Task Set_unknown_provider_returns_400()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PutAsJsonAsync(
            "/api/ai/keys", new SetKeyBody("Bogus", "sk-whatever"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Set_empty_key_returns_400()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PutAsJsonAsync(
            "/api/ai/keys", new SetKeyBody("Anthropic", ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
