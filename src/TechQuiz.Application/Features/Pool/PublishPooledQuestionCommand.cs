using MediatR;

namespace TechQuiz.Application.Features.Pool;

/// <summary>Promotes the caller's own draft pool question into the public pool (ADR-020).</summary>
public sealed record PublishPooledQuestionCommand(Guid PooledQuestionId) : IRequest;
