using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Pool;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Pool;

public class ListPooledQuestionsQueryHandlerTests
{
    private readonly IPooledQuestionRepository _repo = Substitute.For<IPooledQuestionRepository>();

    private ListPooledQuestionsQueryHandler CreateSut() => new(_repo);

    private static PooledQuestion Published()
    {
        var q = PooledQuestion.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Anthropic", "C# records",
            new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc),
            QuestionType.MultipleChoice, Difficulty.Hard, "Are records immutable by default?",
            "Positional records are immutable.",
            [
                new PooledQuestionOption(Guid.NewGuid(), "No", isCorrect: false, orderIndex: 1),
                new PooledQuestionOption(Guid.NewGuid(), "Yes", isCorrect: true, orderIndex: 0),
            ]);
        q.Publish();
        return q;
    }

    [Fact]
    public async Task Handle_ProjectsPublishedQuestions_WithAttribution_OrderedOptions()
    {
        var published = Published();
        _repo.GetPublishedAsync(Arg.Any<CancellationToken>()).Returns([published]);

        var result = await CreateSut().Handle(new ListPooledQuestionsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        var dto = result[0];
        dto.Id.Should().Be(published.Id);
        dto.Stem.Should().Be("Are records immutable by default?");
        dto.Provider.Should().Be("Anthropic");
        dto.Topic.Should().Be("C# records");
        dto.Difficulty.Should().Be(Difficulty.Hard);
        dto.CreatedByUserId.Should().Be(published.CreatedByUserId);
        // Options ordered by index: "Yes" (0) before "No" (1).
        dto.Options.Should().Equal("Yes", "No");
    }

    [Fact]
    public async Task Handle_EmptyPool_ReturnsEmpty()
    {
        _repo.GetPublishedAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await CreateSut().Handle(new ListPooledQuestionsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
