using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class OptionTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var id = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var option = new Option(id, questionId, "internal", isCorrect: true, orderIndex: 2);

        option.Id.Should().Be(id);
        option.QuestionId.Should().Be(questionId);
        option.Text.Should().Be("internal");
        option.IsCorrect.Should().BeTrue();
        option.OrderIndex.Should().Be(2);
    }

    [Fact]
    public void Constructor_PreservesIsCorrectFalse()
    {
        var option = new Option(Guid.NewGuid(), Guid.NewGuid(), "private", isCorrect: false, orderIndex: 0);

        option.IsCorrect.Should().BeFalse();
    }
}
