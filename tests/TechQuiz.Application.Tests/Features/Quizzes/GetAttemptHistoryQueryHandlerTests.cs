using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Quizzes;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class GetAttemptHistoryQueryHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    private GetAttemptHistoryQueryHandler CreateSut() => new(_quizRepository, _userContext);

    private static QuizAttempt SampleAttempt(Guid userId, bool completed = false)
    {
        var attempt = QuizAttempt.Start(
            Guid.NewGuid(),
            userId,
            quizId: Guid.NewGuid(),
            startedAt: new DateTimeOffset(2026, 5, 13, 10, 0, 0, TimeSpan.Zero));
        if (completed)
        {
            attempt.Complete(attempt.StartedAt.AddMinutes(5), scorePercentage: 0d);
        }
        return attempt;
    }

    [Fact]
    public async Task Handle_ProjectsAttemptsToDtos_WithCompletionFlag()
    {
        var userId = Guid.NewGuid();
        var inProgress = SampleAttempt(userId, completed: false);
        var done = SampleAttempt(userId, completed: true);

        _userContext.UserId.Returns(userId);
        _quizRepository.GetAttemptsByUserAsync(userId, 0, 20, Arg.Any<CancellationToken>())
            .Returns([done, inProgress]);

        var result = await CreateSut().Handle(new GetAttemptHistoryQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Single(a => a.AttemptId == done.AttemptId()).IsCompleted.Should().BeTrue();
        result.Single(a => a.AttemptId == inProgress.AttemptId()).IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PassesPaginationCorrectly_AsSkipAndTake()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetAttemptsByUserAsync(userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<QuizAttempt>());

        // Page 3, PageSize 10 → skip 20, take 10.
        await CreateSut().Handle(new GetAttemptHistoryQuery(Page: 3, PageSize: 10), CancellationToken.None);

        await _quizRepository.Received(1).GetAttemptsByUserAsync(
            userId, 20, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ScopesToCurrentUser()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetAttemptsByUserAsync(userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<QuizAttempt>());

        await CreateSut().Handle(new GetAttemptHistoryQuery(), CancellationToken.None);

        await _quizRepository.Received(1).GetAttemptsByUserAsync(
            userId, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyHistory_ReturnsEmptyList()
    {
        _userContext.UserId.Returns(Guid.NewGuid());
        _quizRepository.GetAttemptsByUserAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<QuizAttempt>());

        var result = await CreateSut().Handle(new GetAttemptHistoryQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

internal static class QuizAttemptTestExtensions
{
    // QuizAttempt's Id is exposed as a Domain property; helper for terser test assertions.
    public static Guid AttemptId(this QuizAttempt attempt) => attempt.Id;
}
