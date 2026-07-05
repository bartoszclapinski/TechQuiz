using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Categories;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrackDto>>> Get(CancellationToken cancellationToken)
    {
        var tracks = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(tracks);
    }
}
