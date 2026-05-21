using FluentAssertions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Repositories;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Persistence.Repositories;

[Collection(DatabaseCollection.Name)]
public sealed class CategoryRepositoryTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedByName()
    {
        await using (var seed = CreateDbContext())
        {
            seed.Categories.AddRange(
                new Category(Guid.NewGuid(), "Beta",  "B desc", "icon-b"),
                new Category(Guid.NewGuid(), "Alpha", "A desc", "icon-a"),
                new Category(Guid.NewGuid(), "Gamma", "G desc", "icon-g"));
            await seed.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetAllAsync();

        result.Select(c => c.Name).Should().Equal("Alpha", "Beta", "Gamma");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoCategories()
    {
        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQuestionCountsAsync_ReturnsCountPerCategory()
    {
        var unitTesting = new Category(Guid.NewGuid(), "Unit Testing", "x", "icon");
        var sql = new Category(Guid.NewGuid(), "SQL", "x", "icon");

        await using (var seed = CreateDbContext())
        {
            seed.Categories.AddRange(unitTesting, sql);
            seed.Questions.AddRange(
                CreateQuestion(unitTesting.Id),
                CreateQuestion(unitTesting.Id),
                CreateQuestion(unitTesting.Id),
                CreateQuestion(sql.Id));
            await seed.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetQuestionCountsAsync();

        result.Should().HaveCount(2);
        result[unitTesting.Id].Should().Be(3);
        result[sql.Id].Should().Be(1);
    }

    [Fact]
    public async Task GetQuestionCountsAsync_OmitsCategoriesWithoutQuestions()
    {
        // The GroupBy in the repo never emits a row for a category with zero questions.
        // The Application handler relies on this — it falls back to 0 via TryGetValue.
        var withQuestions = new Category(Guid.NewGuid(), "WithQ", "x", "icon");
        var empty = new Category(Guid.NewGuid(), "Empty", "x", "icon");

        await using (var seed = CreateDbContext())
        {
            seed.Categories.AddRange(withQuestions, empty);
            seed.Questions.Add(CreateQuestion(withQuestions.Id));
            await seed.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetQuestionCountsAsync();

        result.Should().ContainKey(withQuestions.Id);
        result.Should().NotContainKey(empty.Id);
    }

    private static Question CreateQuestion(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Q?",
            explanation: "exp",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "B", isCorrect: false, orderIndex: 1),
            ]);
    }
}
