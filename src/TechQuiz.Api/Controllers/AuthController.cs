using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Auth;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IMediator mediator,
    IWebHostEnvironment environment,
    IConfiguration configuration) : ControllerBase
{
    // The refresh token rides in an HttpOnly cookie so it can't be read back from storage by
    // browser JS — the SPA holds only the access token, in memory. Scoped to the refresh
    // endpoint path so it isn't sent on every other API call.
    // Caveat: these endpoints still return the refresh token in the JSON body (for non-cookie
    // clients like Postman/tests), so it is briefly JS-readable in the auth response itself.
    // Omitting it from the browser response is tracked as a follow-up (see PR/LOG).
    private const string RefreshCookieName = "refresh_token";
    private const string RefreshCookiePath = "/api/auth/refresh";

    // In Development the SPA and API share http://localhost (same-site), so Strict works and keeps the
    // refresh cookie off cross-site requests. In Staging/Production the web and API live on different
    // hosts (e.g. *.onrender.com subdomains — cross-site), so the browser sends the refresh cookie only
    // if it is SameSite=None; that in turn requires Secure (set alongside), which holds over the
    // deployment's HTTPS (ADR-022). Append and Delete must use the same attributes for the browser to
    // clear the cookie, so both read this one value.
    private SameSiteMode CrossSiteCookieMode =>
        environment.IsDevelopment() ? SameSiteMode.Strict : SameSiteMode.None;

    [HttpPost("register")]
    public async Task<ActionResult<AuthTokensDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        // Self-registration is gated by config (Auth:RegistrationEnabled, default false) — closed on
        // the live demo until there is a privacy policy / GDPR story, open in Development for testing
        // (iteration 4.11). The gate lives here so it short-circuits before any user is created.
        if (!configuration.GetValue("Auth:RegistrationEnabled", false))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Registration closed",
                detail: "Public registration is currently disabled. Use the demo account to explore TechQuiz.");
        }

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

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Delete the refresh cookie so the browser can no longer refresh. The token row
        // lingers in the DB until the cleanup job (#68); that's acceptable because the token
        // was only ever held in this HttpOnly cookie, so deleting it leaves no party able to
        // use it. Attributes must match the original cookie for the browser to clear it.
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = CrossSiteCookieMode,
            Path = RefreshCookiePath,
        });
        return NoContent();
    }

    private void SetRefreshCookie(AuthTokensDto tokens)
    {
        Response.Cookies.Append(RefreshCookieName, tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            // Secure requires HTTPS; relaxed in Development so the http://localhost dev flow works.
            // Outside Development SameSite=None (see CrossSiteCookieMode) mandates Secure, which holds
            // over the deployment's HTTPS.
            Secure = !environment.IsDevelopment(),
            SameSite = CrossSiteCookieMode,
            Path = RefreshCookiePath,
            Expires = tokens.RefreshTokenExpiresAt,
        });
    }
}
