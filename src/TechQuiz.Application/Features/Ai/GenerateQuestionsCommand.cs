using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Ai;

public sealed record GenerateQuestionsCommand(
    string Topic,
    Difficulty Difficulty,
    int Count,
    AiProviderKind Provider)
    : IRequest<GenerateQuestionsResult>;
