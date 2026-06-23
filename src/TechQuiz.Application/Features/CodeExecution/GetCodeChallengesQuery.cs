using MediatR;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed record GetCodeChallengesQuery : IRequest<IReadOnlyList<CodeChallengeDto>>;
