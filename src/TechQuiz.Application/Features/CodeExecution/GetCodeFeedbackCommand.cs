using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed record GetCodeFeedbackCommand(
    Guid ChallengeId,
    string SourceCode,
    AiProviderKind Provider)
    : IRequest<GetCodeFeedbackResult>;
