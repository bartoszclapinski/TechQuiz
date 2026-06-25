using FluentAssertions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Ai;

public class GenerateQuestionsCommandValidatorTests
{
    private readonly GenerateQuestionsCommandValidator _validator = new();

    private static GenerateQuestionsCommand Valid() =>
        new("C# generics", Difficulty.Medium, 5, AiProviderKind.Anthropic);

    [Fact]
    public void Valid_Command_Passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTopic_Fails(string topic)
    {
        var result = _validator.Validate(Valid() with { Topic = topic });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateQuestionsCommand.Topic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void CountOutOfRange_Fails(int count)
    {
        var result = _validator.Validate(Valid() with { Count = count });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateQuestionsCommand.Count));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void CountAtBounds_Passes(int count)
    {
        _validator.Validate(Valid() with { Count = count }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnknownDifficulty_Fails()
    {
        var result = _validator.Validate(Valid() with { Difficulty = (Difficulty)99 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GenerateQuestionsCommand.Difficulty));
    }
}
