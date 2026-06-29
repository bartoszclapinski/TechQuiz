using FluentAssertions;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.CodeExecution;

namespace TechQuiz.Application.Tests.Features.CodeExecution;

public class GetCodeFeedbackCommandValidatorTests
{
    private readonly GetCodeFeedbackCommandValidator _validator = new();

    private static GetCodeFeedbackCommand Valid() =>
        new(Guid.NewGuid(), "var x = 1;", AiProviderKind.Anthropic);

    [Fact]
    public void Valid_command_passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_source_code_fails()
    {
        var result = _validator.Validate(Valid() with { SourceCode = "  " });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetCodeFeedbackCommand.SourceCode));
    }

    [Fact]
    public void Unknown_provider_fails()
    {
        var result = _validator.Validate(Valid() with { Provider = (AiProviderKind)999 });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetCodeFeedbackCommand.Provider));
    }
}
