namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Selects the registered <see cref="IAiProvider"/> for a requested
/// <see cref="AiProviderKind"/>. An unregistered kind fails loudly — there is
/// no silent fallback to a different provider (ADR-006).
/// </summary>
public interface IAiProviderResolver
{
    IAiProvider Resolve(AiProviderKind kind);
}
