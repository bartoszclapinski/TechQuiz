using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using TechQuiz.Application.Common.Behaviors;

namespace TechQuiz.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_PassesThrough()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ValidRequest_PassesThroughToHandler()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_FailingValidator_Throws_ValidationException()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required.")]));
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var handlerCalled = false;
        RequestHandlerDelegate<string> next = (ct) =>
        {
            handlerCalled = true;
            return Task.FromResult("ok");
        };

        var act = async () => await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.Any(f => f.ErrorMessage == "Name is required."));
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AggregatesFailuresFromMultipleValidators()
    {
        var v1 = Substitute.For<IValidator<TestRequest>>();
        v1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("A", "a-fail")]));
        var v2 = Substitute.For<IValidator<TestRequest>>();
        v2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("B", "b-fail")]));
        var behavior = new ValidationBehavior<TestRequest, string>([v1, v2]);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        var act = async () => await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Should().HaveCount(2);
    }
}
