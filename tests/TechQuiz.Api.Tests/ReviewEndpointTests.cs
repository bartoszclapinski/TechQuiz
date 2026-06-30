using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TechQuiz.Api.Tests.Support;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class ReviewEndpointTests(TechQuizApiFactory factory)
{
    [Fact]
    public async Task Daily_review_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/review/daily");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Daily_review_with_a_token_returns_200()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/review/daily");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Daily_review_rejects_out_of_range_count()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.GetAsync("/api/review/daily?count=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Grade_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/review/grade", new
        {
            answers = new[] { new { questionId = Guid.NewGuid(), selectedOptionId = (Guid?)null } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Grade_with_empty_answers_returns_400()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PostAsJsonAsync("/api/review/grade", new { answers = Array.Empty<object>() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Grade_with_a_token_returns_200()
    {
        var client = await factory.CreateDemoClientAsync();

        var response = await client.PostAsJsonAsync("/api/review/grade", new
        {
            answers = new[] { new { questionId = Guid.NewGuid(), selectedOptionId = (Guid?)null } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
