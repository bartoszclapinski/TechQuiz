using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechQuiz.Api.Tests.Support;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AiQuestionsEndpointsTests(TechQuizApiFactory factory)
{
    private sealed record GenerateBody(string Topic, string Difficulty, int Count, string Provider);
    private sealed record SetKeyBody(string Provider, string ApiKey);

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
    public async Task Generate_when_provider_rejects_the_key_returns_422_without_leaking_it()
    {
        const string key = "sk-ant-leaky-0123456789abcdef";
        await using var host = WithProviderThrowing(
            new AiProviderRejectedException(AiProviderKind.Anthropic, AiRejectionReason.InvalidKey));
        var client = await host.CreateDemoClientAsync();
        (await client.PutAsJsonAsync("/api/ai/keys", new SetKeyBody("Anthropic", key))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Easy", 3, "Anthropic"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadAsStringAsync();
        problem.Should().NotContain(key).And.NotContain("89abcdef");
    }

    [Fact]
    public async Task Generate_when_provider_is_out_of_quota_returns_429()
    {
        await using var host = WithProviderThrowing(
            new AiProviderRejectedException(AiProviderKind.Anthropic, AiRejectionReason.QuotaOrRateLimit));
        var client = await host.CreateDemoClientAsync();
        (await client.PutAsJsonAsync("/api/ai/keys", new SetKeyBody("Anthropic", "sk-ant-quota"))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync(
            "/api/ai/questions", new GenerateBody("C#", "Easy", 3, "Anthropic"));

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    /// <summary>
    /// A host whose only AI provider throws <paramref name="failure"/>, so the HTTP mapping of
    /// provider failures is tested without calling a real API.
    /// </summary>
    private WebApplicationFactory<Program> WithProviderThrowing(Exception failure) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiProvider>();
            services.AddSingleton<IAiProvider>(new ThrowingAiProvider(failure));
        }));

    private sealed class ThrowingAiProvider(Exception failure) : IAiProvider
    {
        public AiProviderKind Kind => AiProviderKind.Anthropic;

        public Task<IReadOnlyList<GeneratedQuestionDraft>> GenerateQuestionsAsync(
            GenerateQuestionsRequest request, string apiKey, CancellationToken cancellationToken = default) =>
            throw failure;

        public Task<string> GenerateCodeFeedbackAsync(
            CodeFeedbackRequest request, string apiKey, CancellationToken cancellationToken = default) =>
            throw failure;
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
