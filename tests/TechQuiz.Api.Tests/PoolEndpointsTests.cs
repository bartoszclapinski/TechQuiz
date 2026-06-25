using System.Net;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class PoolEndpointsTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Browsing_the_pool_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/pool/questions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Publishing_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/pool/questions/{Guid.NewGuid()}/publish", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Browsing_the_pool_with_a_token_returns_200()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/pool/questions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Publishing_an_unknown_draft_returns_404()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PostAsync($"/api/pool/questions/{Guid.NewGuid()}/publish", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
