using FluentAssertions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Repositories;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Persistence.Repositories;

[Collection(DatabaseCollection.Name)]
public sealed class CategoryRepositoryTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetTracksAsync_ReturnsTracksOrderedByPosition()
    {
        await using (var seed = CreateDbContext())
        {
            seed.Tracks.AddRange(
                new Track(Guid.NewGuid(), "Databases", "D desc", "DB", position: 1),
                new Track(Guid.NewGuid(), ".NET", "N desc", ".NET", position: 0),
                new Track(Guid.NewGuid(), "Front-End", "F desc", "FE", position: 2));
            await seed.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetTracksAsync();

        result.Select(t => t.Name).Should().Equal(".NET", "Databases", "Front-End");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedByPosition()
    {
        var trackId = Guid.NewGuid();

        await using (var seed = CreateDbContext())
        {
            seed.Tracks.Add(new Track(trackId, "Track", "t", "T", position: 0));
            seed.Categories.AddRange(
                new Category(Guid.NewGuid(), trackId, "Gamma", "G desc", "icon-g", position: 2),
                new Category(Guid.NewGuid(), trackId, "Alpha", "A desc", "icon-a", position: 0),
                new Category(Guid.NewGuid(), trackId, "Beta", "B desc", "icon-b", position: 1));
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
        var trackId = Guid.NewGuid();
        var unitTesting = new Category(Guid.NewGuid(), trackId, "Unit Testing", "x", "icon", position: 0);
        var sql = new Category(Guid.NewGuid(), trackId, "SQL", "x", "icon", position: 1);

        await using (var seed = CreateDbContext())
        {
            seed.Tracks.Add(new Track(trackId, "Track", "t", "T", position: 0));
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
        var trackId = Guid.NewGuid();
        var withQuestions = new Category(Guid.NewGuid(), trackId, "WithQ", "x", "icon", position: 0);
        var empty = new Category(Guid.NewGuid(), trackId, "Empty", "x", "icon", position: 1);

        await using (var seed = CreateDbContext())
        {
            seed.Tracks.Add(new Track(trackId, "Track", "t", "T", position: 0));
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

    [Fact]
    public async Task GetUserBestScoresAsync_ReturnsMaxPercentagePerCategory_ScopedToUser()
    {
        var user = await CreateUserAsync();
        var other = await CreateUserAsync();
        var (catA, quizA) = await SeedCategoryWithQuizAsync();
        var (catB, quizB) = await SeedCategoryWithQuizAsync();

        await SeedCompletedAttemptAsync(user, quizA, scorePercentage: 40d);
        await SeedCompletedAttemptAsync(user, quizA, scorePercentage: 90d);
        await SeedCompletedAttemptAsync(user, quizB, scorePercentage: 55d);
        await SeedCompletedAttemptAsync(other, quizA, scorePercentage: 100d);

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetUserBestScoresAsync(user);

        result.Should().HaveCount(2);
        result[catA].Should().Be(90d);
        result[catB].Should().Be(55d);
    }

    [Fact]
    public async Task GetUserBestScoresAsync_OmitsCategoriesWithoutCompletedAttempt()
    {
        var user = await CreateUserAsync();
        var (_, quizA) = await SeedCategoryWithQuizAsync();
        await SeedCategoryWithQuizAsync(); // category B — never attempted
        await SeedInProgressAttemptAsync(user, quizA); // in progress: no score yet

        await using var db = CreateDbContext();
        var sut = new CategoryRepository(db);

        var result = await sut.GetUserBestScoresAsync(user);

        result.Should().BeEmpty();
    }

    private async Task<(Guid CategoryId, Guid QuizId)> SeedCategoryWithQuizAsync()
    {
        var trackId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        await using var db = CreateDbContext();
        db.Tracks.Add(new Track(trackId, $"Track-{trackId:N}", "x", "icon", position: 0));
        db.Categories.Add(new Category(categoryId, trackId, $"Cat-{categoryId:N}", "x", "icon", position: 0));
        db.Quizzes.Add(Quiz.Create(quizId, categoryId, [CreateQuestion(categoryId)]));
        await db.SaveChangesAsync();
        return (categoryId, quizId);
    }

    private async Task SeedCompletedAttemptAsync(Guid userId, Guid quizId, double scorePercentage)
    {
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quizId, DateTimeOffset.UtcNow);
        attempt.Complete(DateTimeOffset.UtcNow.AddMinutes(5), scorePercentage);
        await using var db = CreateDbContext();
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();
    }

    private async Task SeedInProgressAttemptAsync(Guid userId, Guid quizId)
    {
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quizId, DateTimeOffset.UtcNow);
        await using var db = CreateDbContext();
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();
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
