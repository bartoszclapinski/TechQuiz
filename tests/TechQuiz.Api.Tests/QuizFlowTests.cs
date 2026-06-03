using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TechQuiz.Api.Contracts.Quizzes;
using TechQuiz.Api.Tests.Support;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class QuizFlowTests(TechQuizApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Full_quiz_flow_runs_end_to_end_for_the_demo_user()
    {
        var client = await factory.CreateDemoClientAsync();

        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        categories.Should().NotBeNullOrEmpty();
        var categoryId = categories![0].Id;

        var startResponse = await client.PostAsJsonAsync(
            "/api/quizzes/start", new StartQuizRequest(categoryId));
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Hard Rule #4: the active-quiz payload must never reveal which option is correct.
        var startBody = await startResponse.Content.ReadAsStringAsync();
        startBody.ToLowerInvariant().Should().NotContain("iscorrect");

        var session = JsonSerializer.Deserialize<QuizSessionDto>(startBody, JsonOptions)!;
        session.Questions.Should().NotBeEmpty();

        foreach (var question in session.Questions)
        {
            var answer = await client.PostAsJsonAsync(
                $"/api/quizzes/{session.AttemptId}/answer",
                new SubmitAnswerRequest(question.Id, question.Options[0].Id));
            answer.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        var completeResponse = await client.PostAsync(
            $"/api/quizzes/{session.AttemptId}/complete", content: null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resultDocument = JsonDocument.Parse(await completeResponse.Content.ReadAsStringAsync());
        var result = resultDocument.RootElement;
        result.GetProperty("attemptId").GetGuid().Should().Be(session.AttemptId);
        result.GetProperty("totalCount").GetInt32().Should().Be(session.Questions.Count);

        // The result is re-fetchable read-only after completion.
        var resultResponse = await client.GetAsync($"/api/quizzes/{session.AttemptId}/result");
        resultResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
