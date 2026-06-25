using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Lists the provider kinds the current user has a key configured for. Returns
/// kinds only — never the key material (ADR-006).
/// </summary>
public sealed record GetConfiguredProvidersQuery : IRequest<IReadOnlyList<AiProviderKind>>;
