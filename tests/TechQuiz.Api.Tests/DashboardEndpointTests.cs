using System.Net;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class DashboardEndpointTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Dashboard_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Dashboard_with_a_token_returns_200()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
