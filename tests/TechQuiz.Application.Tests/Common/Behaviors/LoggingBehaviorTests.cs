using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TechQuiz.Application.Common.Behaviors;

namespace TechQuiz.Application.Tests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record TestRequest(string Name) : IRequest<string>;

    private readonly ILogger<LoggingBehavior<TestRequest, string>> _logger =
        Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();

    [Fact]
    public async Task Handle_SuccessfulRequest_LogsInformation_AndReturnsResult()
    {
        var behavior = new LoggingBehavior<TestRequest, string>(_logger);
        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        result.Should().Be("ok");
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_HandlerThrows_LogsError_AndRethrows()
    {
        var behavior = new LoggingBehavior<TestRequest, string>(_logger);
        var boom = new InvalidOperationException("boom");
        RequestHandlerDelegate<string> next = (ct) => throw boom;

        var act = async () => await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should().Be(boom);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            boom,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
