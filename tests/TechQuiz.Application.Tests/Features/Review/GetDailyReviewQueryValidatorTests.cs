using FluentValidation.TestHelper;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Application.Tests.Features.Review;

public class GetDailyReviewQueryValidatorTests
{
    private readonly GetDailyReviewQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public void Count_OutOfRange_Fails(int count)
    {
        var result = _validator.TestValidate(new GetDailyReviewQuery(count));

        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Count_WithinRange_Passes(int count)
    {
        var result = _validator.TestValidate(new GetDailyReviewQuery(count));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Default_Passes()
    {
        var result = _validator.TestValidate(new GetDailyReviewQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
