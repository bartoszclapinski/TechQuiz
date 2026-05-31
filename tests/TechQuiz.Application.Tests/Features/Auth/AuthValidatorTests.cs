using FluentAssertions;
using TechQuiz.Application.Features.Auth;

namespace TechQuiz.Application.Tests.Features.Auth;

public class AuthValidatorTests
{
    [Theory]
    [InlineData("", "Password1!", false)]
    [InlineData("not-an-email", "Password1!", false)]
    [InlineData("user@test.local", "", false)]
    [InlineData("user@test.local", "short", false)]
    [InlineData("user@test.local", "Password1!", true)]
    public void RegisterValidator_RejectsInvalidShape(string email, string password, bool expectedValid)
    {
        var sut = new RegisterCommandValidator();

        var result = sut.Validate(new RegisterCommand(email, password));

        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("", "anything", false)]
    [InlineData("bad", "anything", false)]
    [InlineData("user@test.local", "", false)]
    [InlineData("user@test.local", "anything", true)] // login does not enforce password shape
    public void LoginValidator_RejectsInvalidShape(string email, string password, bool expectedValid)
    {
        var sut = new LoginCommandValidator();

        var result = sut.Validate(new LoginCommand(email, password));

        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("any-non-empty-string", true)]
    public void RefreshValidator_RequiresNonEmptyToken(string token, bool expectedValid)
    {
        var sut = new RefreshCommandValidator();

        var result = sut.Validate(new RefreshCommand(token));

        result.IsValid.Should().Be(expectedValid);
    }
}
