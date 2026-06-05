using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Auth;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator, IWebHostEnvironment environment) : ControllerBase
{
    // The refresh token rides in an HttpOnly cookie so browser JS (and any XSS) can never
    // read it — the SPA holds only the access token, in memory. Scoped to the refresh
    // endpoint path so it isn't sent on every other API call.
    private const string RefreshCookieName = "refresh_token";
    private const string RefreshCookiePath = "/api/auth/refresh";

    [HttpPost("register")]
    public async Task<ActionResult<AuthTokensDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(
            new RegisterCommand(request.Email, request.Password), cancellationToken);
        SetRefreshCookie(tokens);
        return Ok(tokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(
            new LoginCommand(request.Email, request.Password), cancellationToken);
        SetRefreshCookie(tokens);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshRequest? request,
        CancellationToken cancellationToken)
    {
        // Prefer the HttpOnly cookie (browser flow); fall back to the body so API clients
        // (Postman, integration tests) without a cookie jar can still refresh.
        var refreshToken = Request.Cookies[RefreshCookieName] ?? request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("No refresh token provided.");
        }

        var tokens = await mediator.Send(new RefreshCommand(refreshToken), cancellationToken);
        SetRefreshCookie(tokens);
        return Ok(tokens);
    }

    private void SetRefreshCookie(AuthTokensDto tokens)
    {
        Response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            // Secure requires HTTPS; relaxed in Development so the http://localhost dev flow works.
            Secure = !environment.IsDevelopment(),
            // localhost:5173 → :8080 is same-site (ports don't change the site), so Strict is
            // still sent on the SPA's same-site refresh call. A cross-domain prod frontend
            // would need SameSite=None; Secure — a Phase 4 deployment concern.
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath,
            Expires = tokens.RefreshTokenExpiresAt,
        });
    }
}
