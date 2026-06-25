using FluentAssertions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Repositories;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Persistence.Repositories;

[Collection(DatabaseCollection.Name)]
public sealed class PooledQuestionRepositoryTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddRangeAsync_PersistsDraftWithOptions_AfterSaveChanges()
    {
        var userId = await CreateUserAsync();
        var question = CreateDraft(userId);

        await using (var writeCtx = CreateDbContext())
        {
            var sut = new PooledQuestionRepository(writeCtx);
            await sut.AddRangeAsync([question]);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var roundTripped = await new PooledQuestionRepository(readCtx).GetByIdAsync(question.Id);

        roundTripped.Should().NotBeNull();
        roundTripped!.CreatedByUserId.Should().Be(userId);
        roundTripped.Provider.Should().Be("Anthropic");
        roundTripped.Topic.Should().Be("EF Core");
        roundTripped.Status.Should().Be(PooledQuestionStatus.Draft);
        roundTripped.Options.Should().HaveCount(2);
        roundTripped.Options.Should().ContainSingle(o => o.IsCorrect);
        roundTripped.Options.Select(o => o.OrderIndex).Should().BeEquivalentTo([0, 1]);
    }

    [Fact]
    public async Task Publish_PersistsStatusTransition_Draft_To_Published()
    {
        var userId = await CreateUserAsync();
        var question = CreateDraft(userId);

        await using (var writeCtx = CreateDbContext())
        {
            await new PooledQuestionRepository(writeCtx).AddRangeAsync([question]);
            await writeCtx.SaveChangesAsync();
        }

        await using (var publishCtx = CreateDbContext())
        {
            var sut = new PooledQuestionRepository(publishCtx);
            var tracked = await sut.GetByIdAsync(question.Id);
            tracked!.Publish();
            await publishCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var roundTripped = await new PooledQuestionRepository(readCtx).GetByIdAsync(question.Id);

        roundTripped!.Status.Should().Be(PooledQuestionStatus.Published);
    }

    [Fact]
    public async Task GetPublishedAsync_ReturnsOnlyPublished_OrderedByGeneratedAtDesc()
    {
        var userId = await CreateUserAsync();
        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var draft = CreateDraft(userId, topic: "Draft topic", generatedAtUtc: baseTime.AddMinutes(30));
        var older = CreateDraft(userId, topic: "Older", generatedAtUtc: baseTime.AddMinutes(10));
        var newer = CreateDraft(userId, topic: "Newer", generatedAtUtc: baseTime.AddMinutes(20));
        older.Publish();
        newer.Publish();

        await using (var writeCtx = CreateDbContext())
        {
            await new PooledQuestionRepository(writeCtx).AddRangeAsync([draft, older, newer]);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var published = await new PooledQuestionRepository(readCtx).GetPublishedAsync();

        published.Should().HaveCount(2);
        published.Select(q => q.Topic).Should().Equal("Newer", "Older");
        published.Should().OnlyContain(q => q.Status == PooledQuestionStatus.Published);
    }

    private static PooledQuestion CreateDraft(
        Guid userId, string topic = "EF Core", DateTime? generatedAtUtc = null)
    {
        return PooledQuestion.Create(
            id: Guid.NewGuid(),
            createdByUserId: userId,
            provider: "Anthropic",
            topic: topic,
            generatedAtUtc: generatedAtUtc ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does DbContext track?",
            explanation: "Change tracking.",
            options:
            [
                new PooledQuestionOption(Guid.NewGuid(), "Entities", isCorrect: true, orderIndex: 0),
                new PooledQuestionOption(Guid.NewGuid(), "Files", isCorrect: false, orderIndex: 1),
            ]);
    }
}
