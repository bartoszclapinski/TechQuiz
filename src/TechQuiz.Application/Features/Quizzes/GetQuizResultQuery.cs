using MediatR;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record GetQuizResultQuery(Guid AttemptId) : IRequest<QuizResultDto>;
