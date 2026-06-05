using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class RefreshCookieTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Login_sets_httponly_refresh_cookie_and_refresh_reads_it()
    {
        // The factory client keeps a cookie jar (HandleCookies defaults to true), so it
        // behaves like a browser: the Set-Cookie from login is replayed on the next request.
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(ApiTestExtensions.DemoEmail, ApiTestExtensions.DemoPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        login.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
        var refreshCookie = setCookies!.Single(c => c.StartsWith("refresh_token="));
        var attributes = refreshCookie.ToLowerInvariant();
        attributes.Should().Contain("httponly", "the SPA must never read the refresh token from JS");
        attributes.Should().Contain("path=/api/auth/refresh");

        // No body — the refresh token must be picked up from the cookie alone.
        var refresh = await client.PostAsync("/api/auth/refresh", content: null);
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_without_cookie_or_body_returns_401()
    {
        var client = factory.CreateClient();

        var refresh = await client.PostAsync("/api/auth/refresh", content: null);

        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_clears_the_refresh_cookie_so_refresh_then_fails()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(ApiTestExtensions.DemoEmail, ApiTestExtensions.DemoPassword));

        var logout = await client.PostAsync("/api/auth/logout", content: null);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        logout.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
        setCookies!.Single(c => c.StartsWith("refresh_token=")).ToLowerInvariant()
            .Should().Contain("expires=", "deleting a cookie sends it back with a past expiry");

        // The cookie jar dropped the refresh token, so a subsequent refresh is unauthorized.
        var refresh = await client.PostAsync("/api/auth/refresh", content: null);
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
