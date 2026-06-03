using System.Net;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AuthorizationTests(TechQuizApiFactory factory)
{
    [Theory]
    [InlineData("/api/categories")]
    [InlineData("/api/attempts")]
    public async Task Secured_get_endpoints_return_401_without_a_token(string url)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Secured_endpoint_returns_200_with_a_valid_token()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
