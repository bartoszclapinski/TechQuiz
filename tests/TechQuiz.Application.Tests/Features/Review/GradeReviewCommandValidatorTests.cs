using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Application.Tests.Features.Review;

public class GradeReviewCommandValidatorTests
{
    private readonly GradeReviewCommandValidator _validator = new();

    private static ReviewAnswerInput Answer() => new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Empty_Answers_Fails()
    {
        var result = _validator.TestValidate(new GradeReviewCommand([]));

        result.ShouldHaveValidationErrorFor(x => x.Answers);
    }

    [Fact]
    public void TooMany_Answers_Fails()
    {
        var answers = Enumerable.Range(0, 51).Select(_ => Answer()).ToList();

        var result = _validator.TestValidate(new GradeReviewCommand(answers));

        result.ShouldHaveValidationErrorFor(x => x.Answers);
    }

    [Fact]
    public void Duplicate_QuestionIds_Fails()
    {
        var id = Guid.NewGuid();
        var answers = new List<ReviewAnswerInput>
        {
            new(id, Guid.NewGuid()),
            new(id, Guid.NewGuid()),
        };

        var result = _validator.TestValidate(new GradeReviewCommand(answers));

        result.ShouldHaveValidationErrorFor(x => x.Answers);
    }

    [Fact]
    public void Empty_QuestionId_Fails()
    {
        var answers = new List<ReviewAnswerInput> { new(Guid.Empty, Guid.NewGuid()) };

        var result = _validator.TestValidate(new GradeReviewCommand(answers));

        result.ShouldHaveValidationErrorFor("Answers[0].QuestionId");
    }

    [Fact]
    public void Valid_Session_Passes()
    {
        var answers = new List<ReviewAnswerInput>
        {
            new(Guid.NewGuid(), Guid.NewGuid()),
            new(Guid.NewGuid(), null),
        };

        var result = _validator.TestValidate(new GradeReviewCommand(answers));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
