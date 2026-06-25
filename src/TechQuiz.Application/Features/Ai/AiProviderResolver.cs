using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Resolves providers from the set registered in DI, indexed by their reported
/// <see cref="IAiProvider.Kind"/>. Built once from the injected provider set;
/// an unregistered kind throws <see cref="UnknownAiProviderException"/>.
/// </summary>
public sealed class AiProviderResolver : IAiProviderResolver
{
    private readonly IReadOnlyDictionary<AiProviderKind, IAiProvider> _providers;

    public AiProviderResolver(IEnumerable<IAiProvider> providers)
    {
        var map = new Dictionary<AiProviderKind, IAiProvider>();
        foreach (var provider in providers)
        {
            if (!map.TryAdd(provider.Kind, provider))
            {
                throw new InvalidOperationException(
                    $"More than one AI provider is registered for '{provider.Kind}'.");
            }
        }

        _providers = map;
    }

    public IAiProvider Resolve(AiProviderKind kind) =>
        _providers.TryGetValue(kind, out var provider)
            ? provider
            : throw new UnknownAiProviderException(kind);
}
