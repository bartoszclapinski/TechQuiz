using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class QuestionTests
{
    private static readonly Guid AnyQuestionId = Guid.NewGuid();
    private static readonly Guid AnyCategoryId = Guid.NewGuid();

    private static IReadOnlyList<Option> TwoValidOptions() =>
    [
        new Option(Guid.NewGuid(), AnyQuestionId, "private", isCorrect: false, orderIndex: 0),
        new Option(Guid.NewGuid(), AnyQuestionId, "internal", isCorrect: true,  orderIndex: 1),
    ];

    [Fact]
    public void Create_WithValidInput_ReturnsQuestion()
    {
        var options = TwoValidOptions();

        var question = Question.Create(
            AnyQuestionId,
            AnyCategoryId,
            QuestionType.MultipleChoice,
            Difficulty.Easy,
            text: "Which keyword makes a member accessible only within the same assembly?",
            explanation: "internal restricts visibility to the declaring assembly.",
            options);

        question.Id.Should().Be(AnyQuestionId);
        question.CategoryId.Should().Be(AnyCategoryId);
        question.Type.Should().Be(QuestionType.MultipleChoice);
        question.Difficulty.Should().Be(Difficulty.Easy);
        question.Options.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Create_WithFewerThanTwoOptions_Throws(int optionCount)
    {
        var options = Enumerable.Range(0, optionCount)
            .Select(i => new Option(Guid.NewGuid(), AnyQuestionId, $"opt{i}", isCorrect: i == 0, orderIndex: i))
            .ToList();

        var act = () => Question.Create(
            AnyQuestionId, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "text", "expl", options);

        act.Should().Throw<InvalidQuestionException>().WithMessage("*at least 2 options*");
    }

    [Fact]
    public void Create_MultipleChoice_WithNoCorrectOption_Throws()
    {
        var options = new[]
        {
            new Option(Guid.NewGuid(), AnyQuestionId, "a", isCorrect: false, orderIndex: 0),
            new Option(Guid.NewGuid(), AnyQuestionId, "b", isCorrect: false, orderIndex: 1),
        };

        var act = () => Question.Create(
            AnyQuestionId, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "text", "expl", options);

        act.Should().Throw<InvalidQuestionException>().WithMessage("*exactly one correct*");
    }

    [Fact]
    public void Create_MultipleChoice_WithMoreThanOneCorrectOption_Throws()
    {
        var options = new[]
        {
            new Option(Guid.NewGuid(), AnyQuestionId, "a", isCorrect: true, orderIndex: 0),
            new Option(Guid.NewGuid(), AnyQuestionId, "b", isCorrect: true, orderIndex: 1),
        };

        var act = () => Question.Create(
            AnyQuestionId, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "text", "expl", options);

        act.Should().Throw<InvalidQuestionException>().WithMessage("*exactly one correct*");
    }

    [Fact]
    public void Create_WithNullOptions_Throws()
    {
        var act = () => Question.Create(
            AnyQuestionId, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "text", "expl", options: null!);

        act.Should().Throw<InvalidQuestionException>().WithMessage("*at least 2 options*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceText_Throws(string text)
    {
        var act = () => Question.Create(
            AnyQuestionId, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            text, "expl", TwoValidOptions());

        act.Should().Throw<InvalidQuestionException>().WithMessage("*text*");
    }
}
