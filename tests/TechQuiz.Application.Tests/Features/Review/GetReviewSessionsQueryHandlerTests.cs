using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Application.Tests.Features.Review;

public class GetReviewSessionsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    public GetReviewSessionsQueryHandlerTests()
    {
        _userContext.UserId.Returns(UserId);
    }

    private GetReviewSessionsQueryHandler CreateSut() => new(_quizRepository, _userContext);

    [Fact]
    public async Task Handle_ReturnsSessions_NewestFirst()
    {
        var older = new ReviewSessionSummary(Guid.NewGuid(), Now.AddDays(-3), 10, 6);
        var newer = new ReviewSessionSummary(Guid.NewGuid(), Now.AddDays(-1), 10, 9);
        // Repository returns them out of order; the handler sorts newest-first.
        _quizRepository.GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>())
            .Returns([older, newer]);

        var result = await CreateSut().Handle(new GetReviewSessionsQuery(), CancellationToken.None);

        result.Select(s => s.Id).Should().Equal(newer.Id, older.Id);
    }

    [Fact]
    public async Task Handle_ScopesToTheCurrentUser()
    {
        _quizRepository.GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateSut().Handle(new GetReviewSessionsQuery(), CancellationToken.None);

        await _quizRepository.Received(1).GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>());
    }
}
