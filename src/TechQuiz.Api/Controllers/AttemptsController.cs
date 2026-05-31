using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/attempts")]
[Authorize]
public sealed class AttemptsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttemptHistoryItemDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var attempts = await mediator.Send(new GetAttemptHistoryQuery(page, pageSize), cancellationToken);
        return Ok(attempts);
    }
}
