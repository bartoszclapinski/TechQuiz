using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class CompleteQuizCommandValidatorTests
{
    private readonly CompleteQuizCommandValidator _validator = new();

    [Fact]
    public void EmptyAttemptId_FailsValidation()
    {
        var result = _validator.TestValidate(new CompleteQuizCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.AttemptId);
    }

    [Fact]
    public void NonEmptyAttemptId_Passes()
    {
        var result = _validator.TestValidate(new CompleteQuizCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
