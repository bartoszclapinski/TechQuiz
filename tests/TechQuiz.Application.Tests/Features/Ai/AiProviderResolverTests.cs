using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class AiProviderResolverTests
{
    private static IAiProvider ProviderOf(AiProviderKind kind)
    {
        var provider = Substitute.For<IAiProvider>();
        provider.Kind.Returns(kind);
        return provider;
    }

    [Fact]
    public void Resolve_RegisteredKind_ReturnsThatProvider()
    {
        var anthropic = ProviderOf(AiProviderKind.Anthropic);
        var openRouter = ProviderOf(AiProviderKind.OpenRouter);
        var resolver = new AiProviderResolver([anthropic, openRouter]);

        resolver.Resolve(AiProviderKind.OpenRouter).Should().BeSameAs(openRouter);
        resolver.Resolve(AiProviderKind.Anthropic).Should().BeSameAs(anthropic);
    }

    [Fact]
    public void Resolve_UnregisteredKind_ThrowsUnknownAiProvider()
    {
        var resolver = new AiProviderResolver([ProviderOf(AiProviderKind.Anthropic)]);

        var act = () => resolver.Resolve(AiProviderKind.OpenRouter);

        act.Should().Throw<UnknownAiProviderException>()
            .Which.Kind.Should().Be(AiProviderKind.OpenRouter);
    }

    [Fact]
    public void Ctor_DuplicateKind_Throws()
    {
        var act = () => new AiProviderResolver(
            [ProviderOf(AiProviderKind.Anthropic), ProviderOf(AiProviderKind.Anthropic)]);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Anthropic*");
    }
}
