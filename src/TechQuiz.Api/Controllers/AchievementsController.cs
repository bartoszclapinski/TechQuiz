using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Achievements;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/achievements")]
[Authorize]
public sealed class AchievementsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AchievementsDto>> Get(CancellationToken cancellationToken = default)
    {
        var achievements = await mediator.Send(new GetAchievementsQuery(), cancellationToken);
        return Ok(achievements);
    }
}
