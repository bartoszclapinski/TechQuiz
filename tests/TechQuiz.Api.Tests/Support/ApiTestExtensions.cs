using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Api.Tests.Support;

internal static class ApiTestExtensions
{
    // Seeded by the dev DataSeeder on startup (see docker-compose / DataSeeder). The relaxed
    // password policy in appsettings.Development.json is what lets this 8-char password stand.
    public const string DemoEmail = "demo@techquiz.local";
    public const string DemoPassword = "Demo123!";

    /// <summary>
    /// Returns a client whose Authorization header carries a fresh demo-user JWT — the same
    /// login path the SPA uses, so the secured endpoints see a real bearer token.
    /// </summary>
    public static async Task<HttpClient> CreateDemoClientAsync(this TechQuizApiFactory factory)
    {
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(DemoEmail, DemoPassword));
        login.EnsureSuccessStatusCode();

        var tokens = await login.Content.ReadFromJsonAsync<AuthTokensDto>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        return client;
    }
}
