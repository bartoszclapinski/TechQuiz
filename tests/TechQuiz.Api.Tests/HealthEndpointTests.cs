using System.Net;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class HealthEndpointTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Health_returns_200_when_the_host_and_database_are_up()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
