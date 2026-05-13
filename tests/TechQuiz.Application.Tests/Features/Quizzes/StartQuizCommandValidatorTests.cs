using FluentAssertions;
using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class StartQuizCommandValidatorTests
{
    private readonly StartQuizCommandValidator _validator = new();

    [Fact]
    public void EmptyCategoryId_FailsValidation()
    {
        var result = _validator.TestValidate(new StartQuizCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void NonEmptyCategoryId_PassesValidation()
    {
        var result = _validator.TestValidate(new StartQuizCommand(Guid.NewGuid()));

        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }
}
