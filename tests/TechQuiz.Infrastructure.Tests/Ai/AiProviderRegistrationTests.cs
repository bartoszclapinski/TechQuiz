using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Ai;

namespace TechQuiz.Infrastructure.Tests.Ai;

// Exercises resolver construction over an injected IEnumerable<IAiProvider> with a stub, so
// no network is involved. The real AddInfrastructure AI wiring (named typed clients, each
// provider's host) is covered by AiProviderWiringTests.
public class AiProviderRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAiProvider, StubAiProvider>();
        services.AddSingleton<IAiProviderResolver, AiProviderResolver>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Resolver_ResolvesAnthropic_ToTheRegisteredStub()
    {
        using var provider = BuildProvider();

        var resolved = provider.GetRequiredService<IAiProviderResolver>()
            .Resolve(AiProviderKind.Anthropic);

        resolved.Should().BeOfType<StubAiProvider>();
    }

    [Fact]
    public async Task ResolvedStub_ReturnsDeterministicDrafts_OnePerRequestedCount()
    {
        using var provider = BuildProvider();
        var stub = provider.GetRequiredService<IAiProviderResolver>()
            .Resolve(AiProviderKind.Anthropic);

        var drafts = await stub.GenerateQuestionsAsync(
            new GenerateQuestionsRequest("LINQ", Difficulty.Medium, 3), "unused-key");

        drafts.Should().HaveCount(3);
        drafts.Should().OnlyContain(d => d.Options.Count == 4 && d.Difficulty == Difficulty.Medium);
    }
}
