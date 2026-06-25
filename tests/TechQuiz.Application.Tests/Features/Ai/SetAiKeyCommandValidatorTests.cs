using FluentAssertions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class SetAiKeyCommandValidatorTests
{
    private readonly SetAiKeyCommandValidator _validator = new();

    private static SetAiKeyCommand Valid() => new(AiProviderKind.Anthropic, "sk-ant-123");

    [Fact]
    public void Valid_Command_Passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyApiKey_Fails(string apiKey)
    {
        var result = _validator.Validate(Valid() with { ApiKey = apiKey });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetAiKeyCommand.ApiKey));
    }

    [Fact]
    public void UnknownProvider_Fails()
    {
        var result = _validator.Validate(Valid() with { Provider = (AiProviderKind)99 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetAiKeyCommand.Provider));
    }
}
