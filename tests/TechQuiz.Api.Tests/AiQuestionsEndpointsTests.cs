using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AiQuestionsEndpointsTests(TechQuizApiFactory factory)
{
    private sealed record GenerateBody(string Topic, string Difficulty, int Count, string Provider);

    [Fact]
    public async Task Generate_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Easy", 3, "Anthropic"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_without_a_configured_key_returns_409()
    {
        var client = await factory.CreateDemoClientAsync();
        // Guarantee the no-key state regardless of other tests' ordering.
        await client.DeleteAsync("/api/ai/keys/Anthropic");

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Easy", 3, "Anthropic"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Generate_with_unknown_provider_returns_400()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Easy", 3, "Bogus"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Generate_with_unknown_difficulty_returns_400()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Trivial", 3, "Anthropic"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
