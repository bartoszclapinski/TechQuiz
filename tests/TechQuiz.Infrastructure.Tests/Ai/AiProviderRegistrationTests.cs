using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Ai;

namespace TechQuiz.Infrastructure.Tests.Ai;

// Exercises the AI provider registration shape from Infrastructure DI through a real
// container. AddInfrastructure as a whole is DB/Identity/Judge0-bound, so this mirrors
// just the two AI lines — enough to prove IEnumerable<IAiProvider> injection and resolver
// construction without spinning up Postgres or hitting a network.
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
