using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Auth;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthTokensDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(
            new RegisterCommand(request.Email, request.Password), cancellationToken);
        return Ok(tokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(
            new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(
            new RefreshCommand(request.RefreshToken), cancellationToken);
        return Ok(tokens);
    }
}
