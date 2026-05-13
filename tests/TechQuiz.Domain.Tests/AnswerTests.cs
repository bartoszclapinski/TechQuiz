using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class AnswerTests
{
    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var answer = new Answer(questionId, optionId, T0);

        answer.QuestionId.Should().Be(questionId);
        answer.SelectedOptionId.Should().Be(optionId);
        answer.SubmittedAt.Should().Be(T0);
    }

    [Fact]
    public void Constructor_AcceptsNullSelectedOption_ForUnanswered()
    {
        var answer = new Answer(Guid.NewGuid(), selectedOptionId: null, T0);

        answer.SelectedOptionId.Should().BeNull();
    }
}
