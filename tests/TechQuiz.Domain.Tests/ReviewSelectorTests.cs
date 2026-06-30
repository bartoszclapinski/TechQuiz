using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class ReviewSelectorTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    private static ReviewCandidate Candidate(
        Guid questionId,
        bool wasCorrect,
        Difficulty difficulty = Difficulty.Easy,
        double daysAgo = 1)
        => new(questionId, difficulty, Now.AddDays(-daysAgo), wasCorrect);

    [Fact]
    public void Empty_ReturnsEmpty()
    {
        var result = ReviewSelector.SelectDailyReview([], count: 10, now: Now);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveCount_ReturnsEmpty(int count)
    {
        var result = ReviewSelector.SelectDailyReview(
            [Candidate(Guid.NewGuid(), wasCorrect: false)], count, Now);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AllLatestCorrect_ReturnsEmpty()
    {
        var candidates = new[]
        {
            Candidate(Guid.NewGuid(), wasCorrect: true),
            Candidate(Guid.NewGuid(), wasCorrect: true),
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().BeEmpty();
    }

    [Fact]
    public void IncludesQuestionWhoseLatestAnswerWasWrong()
    {
        var q = Guid.NewGuid();

        var result = ReviewSelector.SelectDailyReview(
            [Candidate(q, wasCorrect: false)], count: 10, now: Now);

        result.Should().ContainSingle().Which.Should().Be(q);
    }

    [Fact]
    public void WrongThenLaterCorrect_IsExcluded_LatestWins()
    {
        var q = Guid.NewGuid();
        var candidates = new[]
        {
            Candidate(q, wasCorrect: false, daysAgo: 5),
            Candidate(q, wasCorrect: true, daysAgo: 1), // latest → learned
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().BeEmpty();
    }

    [Fact]
    public void CorrectThenLaterWrong_IsIncluded_LatestWins()
    {
        var q = Guid.NewGuid();
        var candidates = new[]
        {
            Candidate(q, wasCorrect: true, daysAgo: 5),
            Candidate(q, wasCorrect: false, daysAgo: 1), // latest → still wrong
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().ContainSingle().Which.Should().Be(q);
    }

    [Fact]
    public void HarderQuestionRanksAboveEasier_SameRecency()
    {
        var easy = Guid.NewGuid();
        var hard = Guid.NewGuid();
        var candidates = new[]
        {
            Candidate(easy, wasCorrect: false, difficulty: Difficulty.Easy, daysAgo: 2),
            Candidate(hard, wasCorrect: false, difficulty: Difficulty.Hard, daysAgo: 2),
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().Equal(hard, easy);
    }

    [Fact]
    public void OlderWrongRanksAboveNewer_SameDifficulty()
    {
        var recent = Guid.NewGuid();
        var old = Guid.NewGuid();
        var candidates = new[]
        {
            Candidate(recent, wasCorrect: false, difficulty: Difficulty.Medium, daysAgo: 1),
            Candidate(old, wasCorrect: false, difficulty: Difficulty.Medium, daysAgo: 20),
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().Equal(old, recent);
    }

    [Fact]
    public void CapsToCount_TakingTopWeighted()
    {
        var hard = Guid.NewGuid();
        var medium = Guid.NewGuid();
        var easy = Guid.NewGuid();
        var candidates = new[]
        {
            Candidate(easy, wasCorrect: false, difficulty: Difficulty.Easy, daysAgo: 1),
            Candidate(hard, wasCorrect: false, difficulty: Difficulty.Hard, daysAgo: 1),
            Candidate(medium, wasCorrect: false, difficulty: Difficulty.Medium, daysAgo: 1),
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 2, now: Now);

        result.Should().Equal(hard, medium);
    }

    [Fact]
    public void TiesBrokenByQuestionId_ForStableOrdering()
    {
        var a = new Guid("00000000-0000-0000-0000-000000000001");
        var b = new Guid("00000000-0000-0000-0000-000000000002");
        var candidates = new[]
        {
            Candidate(b, wasCorrect: false, difficulty: Difficulty.Easy, daysAgo: 3),
            Candidate(a, wasCorrect: false, difficulty: Difficulty.Easy, daysAgo: 3),
        };

        var result = ReviewSelector.SelectDailyReview(candidates, count: 10, now: Now);

        result.Should().Equal(a, b);
    }
}
