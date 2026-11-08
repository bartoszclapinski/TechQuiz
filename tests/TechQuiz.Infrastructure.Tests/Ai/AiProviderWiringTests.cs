using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Tests.Ai;

/// <summary>
/// Exercises the real <c>AddInfrastructure</c> AI wiring. Each provider's typed client must keep
/// its own base address: two unnamed <c>AddHttpClient&lt;IAiProvider, …&gt;</c> registrations share
/// one client name, the last <c>BaseAddress</c> wins, and a provider silently calls another
/// vendor's API. Resolving the AI services never opens the database, so a dummy connection
/// string is enough.
/// </summary>
public sealed class AiProviderWiringTests
{
    private readonly CapturingHandler _handler = new();

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=unused;Database=unused",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => _handler));
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(AiProviderKind.Anthropic, "api.anthropic.com")]
    public async Task ResolvedProvider_SendsRequestsToItsOwnHost(AiProviderKind kind, string expectedHost)
    {
        _handler.RespondWith("{}");
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var ai = scope.ServiceProvider.GetRequiredService<IAiProviderResolver>().Resolve(kind);

        // The canned "{}" reply is not a valid completion; only the outgoing request matters here.
        var act = () => ai.GenerateQuestionsAsync(new GenerateQuestionsRequest("LINQ", Difficulty.Easy, 1), "key");
        await act.Should().ThrowAsync<Exception>();

        _handler.LastRequest!.RequestUri!.Host.Should().Be(expectedHost);
    }
}
