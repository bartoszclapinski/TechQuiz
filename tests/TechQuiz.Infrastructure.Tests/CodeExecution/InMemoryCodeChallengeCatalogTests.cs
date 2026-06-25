using FluentAssertions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.CodeExecution;

namespace TechQuiz.Infrastructure.Tests.CodeExecution;

public class InMemoryCodeChallengeCatalogTests
{
    private readonly InMemoryCodeChallengeCatalog _catalog = new();

    [Fact]
    public void GetAll_ReturnsSeededChallengesWithDistinctIds()
    {
        var challenges = _catalog.GetAll();

        challenges.Should().NotBeEmpty();
        challenges.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        challenges.Should().OnlyContain(c => c.TestCases.Count >= 1);
        challenges.Should().Contain(c => c.Difficulty == Difficulty.Easy);
        challenges.Should().Contain(c => c.Difficulty == Difficulty.Hard);
    }

    [Fact]
    public void Find_WithSeededId_ReturnsChallenge()
    {
        var expected = _catalog.GetAll()[0];

        _catalog.Find(expected.Id).Should().BeSameAs(expected);
    }

    [Fact]
    public void Find_WithUnknownId_ReturnsNull()
    {
        _catalog.Find(Guid.NewGuid()).Should().BeNull();
    }
}
