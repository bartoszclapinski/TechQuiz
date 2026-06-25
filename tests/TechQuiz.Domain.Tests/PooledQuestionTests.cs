using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class PooledQuestionTests
{
    private static readonly Guid AnyId = Guid.NewGuid();
    private static readonly Guid AnyUserId = Guid.NewGuid();
    private static readonly DateTime AnyGeneratedAt = new(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<PooledQuestionOption> TwoValidOptions() =>
    [
        new PooledQuestionOption(Guid.NewGuid(), "private", isCorrect: false, orderIndex: 0),
        new PooledQuestionOption(Guid.NewGuid(), "internal", isCorrect: true, orderIndex: 1),
    ];

    private static PooledQuestion CreateValid(
        IReadOnlyList<PooledQuestionOption>? options = null,
        string text = "Which keyword limits visibility to the declaring assembly?",
        string provider = "Anthropic",
        string topic = "C# access modifiers") =>
        PooledQuestion.Create(
            AnyId,
            AnyUserId,
            provider,
            topic,
            AnyGeneratedAt,
            QuestionType.MultipleChoice,
            Difficulty.Easy,
            text,
            explanation: "internal restricts visibility to the declaring assembly.",
            options ?? TwoValidOptions());

    [Fact]
    public void Create_WithValidInput_ReturnsDraftWithAttribution()
    {
        var pooled = CreateValid();

        pooled.Id.Should().Be(AnyId);
        pooled.CreatedByUserId.Should().Be(AnyUserId);
        pooled.Provider.Should().Be("Anthropic");
        pooled.Topic.Should().Be("C# access modifiers");
        pooled.GeneratedAtUtc.Should().Be(AnyGeneratedAt);
        pooled.Type.Should().Be(QuestionType.MultipleChoice);
        pooled.Difficulty.Should().Be(Difficulty.Easy);
        pooled.Options.Should().HaveCount(2);
        pooled.Status.Should().Be(PooledQuestionStatus.Draft);
    }

    [Fact]
    public void Create_AllowsNullExplanation()
    {
        var pooled = PooledQuestion.Create(
            AnyId, AnyUserId, "Anthropic", "topic", AnyGeneratedAt,
            QuestionType.MultipleChoice, Difficulty.Easy, "text", explanation: null, TwoValidOptions());

        pooled.Explanation.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Create_WithFewerThanTwoOptions_Throws(int optionCount)
    {
        var options = Enumerable.Range(0, optionCount)
            .Select(i => new PooledQuestionOption(Guid.NewGuid(), $"opt{i}", isCorrect: i == 0, orderIndex: i))
            .ToList();

        var act = () => CreateValid(options);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*at least 2 options*");
    }

    [Fact]
    public void Create_WithNullOptions_Throws()
    {
        var act = () => PooledQuestion.Create(
            AnyId, AnyUserId, "Anthropic", "topic", AnyGeneratedAt,
            QuestionType.MultipleChoice, Difficulty.Easy, "text", "expl", options: null!);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*at least 2 options*");
    }

    [Fact]
    public void Create_MultipleChoice_WithNoCorrectOption_Throws()
    {
        var options = new[]
        {
            new PooledQuestionOption(Guid.NewGuid(), "a", isCorrect: false, orderIndex: 0),
            new PooledQuestionOption(Guid.NewGuid(), "b", isCorrect: false, orderIndex: 1),
        };

        var act = () => CreateValid(options);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*exactly one correct*");
    }

    [Fact]
    public void Create_MultipleChoice_WithMoreThanOneCorrectOption_Throws()
    {
        var options = new[]
        {
            new PooledQuestionOption(Guid.NewGuid(), "a", isCorrect: true, orderIndex: 0),
            new PooledQuestionOption(Guid.NewGuid(), "b", isCorrect: true, orderIndex: 1),
        };

        var act = () => CreateValid(options);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*exactly one correct*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceText_Throws(string text)
    {
        var act = () => CreateValid(text: text);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*text*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyProvider_Throws(string provider)
    {
        var act = () => CreateValid(provider: provider);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*provider*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTopic_Throws(string topic)
    {
        var act = () => CreateValid(topic: topic);

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*topic*");
    }

    [Fact]
    public void Create_WithEmptyCreatedByUserId_Throws()
    {
        var act = () => PooledQuestion.Create(
            AnyId, Guid.Empty, "Anthropic", "topic", AnyGeneratedAt,
            QuestionType.MultipleChoice, Difficulty.Easy, "text", "expl", TwoValidOptions());

        act.Should().Throw<InvalidPooledQuestionException>().WithMessage("*user*");
    }

    [Fact]
    public void Publish_FromDraft_TransitionsToPublished()
    {
        var pooled = CreateValid();

        pooled.Publish();

        pooled.Status.Should().Be(PooledQuestionStatus.Published);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_Throws()
    {
        var pooled = CreateValid();
        pooled.Publish();

        var act = () => pooled.Publish();

        act.Should().Throw<PooledQuestionAlreadyPublishedException>();
    }
}
