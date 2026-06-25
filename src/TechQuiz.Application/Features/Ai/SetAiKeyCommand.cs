using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>Stores or rotates the current user's API key for a provider.</summary>
public sealed record SetAiKeyCommand(AiProviderKind Provider, string ApiKey) : IRequest;
