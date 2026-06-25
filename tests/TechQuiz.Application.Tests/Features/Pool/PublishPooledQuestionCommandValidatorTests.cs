using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Pool;

namespace TechQuiz.Application.Tests.Features.Pool;

public class PublishPooledQuestionCommandValidatorTests
{
    private readonly PublishPooledQuestionCommandValidator _validator = new();

    [Fact]
    public void EmptyId_IsInvalid()
    {
        var result = _validator.TestValidate(new PublishPooledQuestionCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.PooledQuestionId);
    }

    [Fact]
    public void NonEmptyId_IsValid()
    {
        var result = _validator.TestValidate(new PublishPooledQuestionCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
