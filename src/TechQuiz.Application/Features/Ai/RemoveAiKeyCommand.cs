using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>Removes the current user's stored API key for a provider, if any.</summary>
public sealed record RemoveAiKeyCommand(AiProviderKind Provider) : IRequest;
