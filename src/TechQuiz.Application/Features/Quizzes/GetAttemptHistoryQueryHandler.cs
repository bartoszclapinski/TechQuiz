using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Quizzes;

public sealed class GetAttemptHistoryQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetAttemptHistoryQuery, IReadOnlyList<AttemptHistoryItemDto>>
{
    public async Task<IReadOnlyList<AttemptHistoryItemDto>> Handle(
        GetAttemptHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var attempts = await quizRepository.GetAttemptsByUserAsync(
            userContext.UserId, skip, request.PageSize, cancellationToken);

        return attempts
            .Select(a => new AttemptHistoryItemDto(
                a.Id,
                a.QuizId,
                a.StartedAt,
                a.CompletedAt,
                a.IsCompleted))
            .ToList();
    }
}
