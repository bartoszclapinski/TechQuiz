using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TechQuiz.Api.Contracts.Auth;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AuthEndpointsTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Register_with_invalid_payload_returns_400_problem_details_with_field_errors()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("not-an-email", "short"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("status").GetInt32().Should().Be(400);

        var errors = root.GetProperty("errors");
        errors.TryGetProperty("Email", out _).Should().BeTrue();
        errors.TryGetProperty("Password", out _).Should().BeTrue();
    }
}
