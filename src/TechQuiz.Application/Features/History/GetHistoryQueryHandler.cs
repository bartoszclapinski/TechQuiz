using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.History;

public sealed class GetHistoryQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetHistoryQuery, IReadOnlyList<HistoryItemDto>>
{
    public Task<IReadOnlyList<HistoryItemDto>> Handle(
        GetHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        return quizRepository.GetCompletedHistoryPageAsync(
            userContext.UserId,
            request.Category,
            request.SortBy,
            request.Descending,
            skip,
            request.PageSize,
            cancellationToken);
    }
}
