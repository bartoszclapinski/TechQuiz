using MediatR;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record CompleteQuizCommand(Guid AttemptId) : IRequest<QuizResultDto>;
