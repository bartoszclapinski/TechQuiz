using MediatR;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record StartQuizCommand(Guid CategoryId) : IRequest<QuizSessionDto>;
