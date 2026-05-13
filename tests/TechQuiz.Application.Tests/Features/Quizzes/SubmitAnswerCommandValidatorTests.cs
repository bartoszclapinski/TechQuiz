using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class SubmitAnswerCommandValidatorTests
{
    private readonly SubmitAnswerCommandValidator _validator = new();

    [Fact]
    public void EmptyAttemptId_FailsValidation()
    {
        var result = _validator.TestValidate(
            new SubmitAnswerCommand(Guid.Empty, Guid.NewGuid(), null));

        result.ShouldHaveValidationErrorFor(x => x.AttemptId);
    }

    [Fact]
    public void EmptyQuestionId_FailsValidation()
    {
        var result = _validator.TestValidate(
            new SubmitAnswerCommand(Guid.NewGuid(), Guid.Empty, null));

        result.ShouldHaveValidationErrorFor(x => x.QuestionId);
    }

    [Fact]
    public void NullSelectedOptionId_IsAllowed()
    {
        var result = _validator.TestValidate(
            new SubmitAnswerCommand(Guid.NewGuid(), Guid.NewGuid(), SelectedOptionId: null));

        result.ShouldNotHaveValidationErrorFor(x => x.SelectedOptionId);
    }

    [Fact]
    public void AllRequiredFieldsSet_Passes()
    {
        var result = _validator.TestValidate(
            new SubmitAnswerCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
