using MediatR;

namespace TechQuiz.Application.Features.Pool;

/// <summary>Lists the published questions in the public pool (ADR-007). No answer key.</summary>
public sealed record ListPooledQuestionsQuery : IRequest<IReadOnlyList<PooledQuestionDto>>;
