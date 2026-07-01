using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class AchievementsEndpointTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Achievements_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/achievements");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Achievements_with_a_token_returns_catalogue_and_rollup()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/achievements");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AchievementsDto>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().Be(body.Items.Count);
        body.TotalCount.Should().BeGreaterThan(0);
        body.UnlockedCount.Should().BeInRange(0, body.TotalCount);
        body.Items.Should().OnlyContain(i => i.Progress <= i.Target);
    }
}
