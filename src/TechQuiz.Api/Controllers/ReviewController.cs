using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/review")]
[Authorize]
public sealed class ReviewController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily")]
    public async Task<ActionResult<IReadOnlyList<ReviewQuestionDto>>> GetDaily(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var questions = await mediator.Send(new GetDailyReviewQuery(count), cancellationToken);
        return Ok(questions);
    }

    [HttpPost("grade")]
    public async Task<ActionResult<IReadOnlyList<ReviewGradeResultDto>>> Grade(
        [FromBody] GradeReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var results = await mediator.Send(command, cancellationToken);
        return Ok(results);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ReviewStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var stats = await mediator.Send(new GetReviewStatsQuery(), cancellationToken);
        return Ok(stats);
    }
}
