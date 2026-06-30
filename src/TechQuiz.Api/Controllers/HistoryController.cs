using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.History;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/history")]
[Authorize]
public sealed class HistoryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HistoryItemDto>>> Get(
        [FromQuery] string? category = null,
        [FromQuery] HistorySortField sortBy = HistorySortField.Date,
        [FromQuery] bool descending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var items = await mediator.Send(
            new GetHistoryQuery(category, sortBy, descending, page, pageSize), cancellationToken);
        return Ok(items);
    }
}
