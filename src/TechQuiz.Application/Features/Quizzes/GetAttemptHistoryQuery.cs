using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record GetAttemptHistoryQuery(int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<AttemptHistoryItemDto>>;
