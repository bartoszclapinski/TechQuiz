using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class DomainExceptionTests
{
    [Fact]
    public void InvalidQuestionException_InheritsFromDomainException()
    {
        var ex = new InvalidQuestionException("any");

        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Be("any");
    }

    [Fact]
    public void QuizAlreadyCompletedException_InheritsFromDomainException()
    {
        var ex = new QuizAlreadyCompletedException();

        ex.Should().BeAssignableTo<DomainException>();
        ex.Message.Should().Contain("already completed");
    }

    [Fact]
    public void QuizAlreadyCompletedException_AcceptsCustomMessage()
    {
        var ex = new QuizAlreadyCompletedException("custom");

        ex.Message.Should().Be("custom");
    }
}
