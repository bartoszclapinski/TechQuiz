using MediatR;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed record GradeCodeChallengeCommand(Guid ChallengeId, string SourceCode)
    : IRequest<CodeChallengeGradeResult>;
