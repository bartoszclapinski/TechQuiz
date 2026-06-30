using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.History;

public sealed record GetHistoryQuery(
    string? Category = null,
    HistorySortField SortBy = HistorySortField.Date,
    bool Descending = true,
    int Page = 1,
    int PageSize = 20)
    : IRequest<IReadOnlyList<HistoryItemDto>>;
