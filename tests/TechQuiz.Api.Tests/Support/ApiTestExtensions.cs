using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Api.Tests.Support;

internal static class ApiTestExtensions
{
    // Seeded by the DataSeeder on startup (see docker-compose / DataSeeder). The password
    // satisfies the strict 12-char policy from appsettings.json, so the demo user seeds under
    // any environment's policy.
    public const string DemoEmail = "demo@techquiz.local";
    public const string DemoPassword = "DemoPass123!";

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
