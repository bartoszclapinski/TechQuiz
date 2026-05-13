using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class GetAttemptHistoryQueryValidatorTests
{
    private readonly GetAttemptHistoryQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_LessThan1_Fails(int page)
    {
        var result = _validator.TestValidate(new GetAttemptHistoryQuery(Page: page, PageSize: 20));

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-5)]
    public void PageSize_OutOfRange_Fails(int pageSize)
    {
        var result = _validator.TestValidate(new GetAttemptHistoryQuery(Page: 1, PageSize: pageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Defaults_ArePass()
    {
        var result = _validator.TestValidate(new GetAttemptHistoryQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Page1_Size100_Passes()
    {
        var result = _validator.TestValidate(new GetAttemptHistoryQuery(Page: 1, PageSize: 100));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
