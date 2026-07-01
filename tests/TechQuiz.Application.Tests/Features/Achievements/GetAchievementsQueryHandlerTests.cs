using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Achievements;

namespace TechQuiz.Application.Tests.Features.Achievements;

public class GetAchievementsQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    public GetAchievementsQueryHandlerTests()
    {
        _userContext.UserId.Returns(UserId);
    }

    private GetAchievementsQueryHandler CreateSut() => new(_quizRepository, _userContext);

    [Fact]
    public async Task Handle_ScopesBothReadsToTheCurrentUser()
    {
        _quizRepository.GetCompletedAttemptsWithCategoryAsync(UserId, Arg.Any<CancellationToken>())
            .Returns([]);
        _quizRepository.GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateSut().Handle(new GetAchievementsQuery(), CancellationToken.None);

        await _quizRepository.Received(1)
            .GetCompletedAttemptsWithCategoryAsync(UserId, Arg.Any<CancellationToken>());
        await _quizRepository.Received(1)
            .GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FeedsBothReadsIntoTheCatalogue_AndRollsUpUnlocked()
    {
        var attempts = new[]
        {
            new CompletedAttemptRow(Guid.NewGuid(), "C#", 100d, Now, 5),
        };
        var reviews = new[]
        {
            new ReviewSessionSummary(Guid.NewGuid(), Now, 5, 3),
        };
        _quizRepository.GetCompletedAttemptsWithCategoryAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(attempts);
        _quizRepository.GetReviewSessionSummariesAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(reviews);

        var result = await CreateSut().Handle(new GetAchievementsQuery(), CancellationToken.None);

        // first_quiz, perfect_quiz, first_review all cross their target of 1.
        result.TotalCount.Should().Be(result.Items.Count);
        result.UnlockedCount.Should().Be(3);
        result.Items.Where(i => i.Unlocked).Select(i => i.Key)
            .Should().BeEquivalentTo("first_quiz", "perfect_quiz", "first_review");
    }
}
