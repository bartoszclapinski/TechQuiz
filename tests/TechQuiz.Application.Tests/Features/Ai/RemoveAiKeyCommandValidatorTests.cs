using FluentAssertions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class RemoveAiKeyCommandValidatorTests
{
    private readonly RemoveAiKeyCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_Passes()
    {
        _validator.Validate(new RemoveAiKeyCommand(AiProviderKind.OpenRouter)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnknownProvider_Fails()
    {
        var result = _validator.Validate(new RemoveAiKeyCommand((AiProviderKind)99));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveAiKeyCommand.Provider));
    }
}
