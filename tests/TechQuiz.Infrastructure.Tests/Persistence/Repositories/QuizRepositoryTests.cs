using FluentAssertions;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Repositories;
using TechQuiz.Infrastructure.Tests.Support;

namespace TechQuiz.Infrastructure.Tests.Persistence.Repositories;

[Collection(DatabaseCollection.Name)]
public sealed class QuizRepositoryTests(PostgresContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetByCategoryAsync_ReturnsQuizWithFullGraph()
    {
        var (categoryId, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 2);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetByCategoryAsync(categoryId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(quizId);
        result.Questions.Should().HaveCount(2);
        result.Questions.Should().OnlyContain(q => q.Options.Count == 2);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsNull_WhenCategoryHasNoQuiz()
    {
        var loneCategoryId = Guid.NewGuid();
        await using (var seed = CreateDbContext())
        {
            seed.Categories.Add(new Category(loneCategoryId, "LoneCategory", "x", "icon"));
            await seed.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetByCategoryAsync(loneCategoryId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsQuizWithFullGraph()
    {
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 3);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetByIdAsync(quizId);

        result.Should().NotBeNull();
        result!.Questions.Should().HaveCount(3);
        result.Questions.SelectMany(q => q.Options).Should().HaveCount(6);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenQuizMissing()
    {
        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAttemptAsync_PersistsAttempt_AfterSaveChanges()
    {
        var userId = await SeedUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);

        var attemptId = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;
        var attempt = QuizAttempt.Start(attemptId, userId, quizId, startedAt);

        await using (var writeCtx = CreateDbContext())
        {
            var sut = new QuizRepository(writeCtx);
            await sut.AddAttemptAsync(attempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var roundTripped = await new QuizRepository(readCtx).GetAttemptAsync(attemptId);

        roundTripped.Should().NotBeNull();
        roundTripped!.UserId.Should().Be(userId);
        roundTripped.QuizId.Should().Be(quizId);
        roundTripped.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetAttemptAsync_LoadsOwnedAnswers()
    {
        var userId = await SeedUserAsync();
        var (_, quizId, questionIds) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var questionId = questionIds[0];

        var attemptId = Guid.NewGuid();
        var attempt = QuizAttempt.Start(attemptId, userId, quizId, DateTimeOffset.UtcNow);
        attempt.SubmitAnswer(questionId, selectedOptionId: Guid.NewGuid(), submittedAt: DateTimeOffset.UtcNow);

        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.Add(attempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var roundTripped = await new QuizRepository(readCtx).GetAttemptAsync(attemptId);

        roundTripped.Should().NotBeNull();
        roundTripped!.Answers.Should().HaveCount(1);
        roundTripped.Answers[0].QuestionId.Should().Be(questionId);
    }

    [Fact]
    public async Task GetAttemptsByUserAsync_ScopesToUser_OrdersDescByStartedAt_AndPaginates()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);

        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.AddRange(
                QuizAttempt.Start(Guid.NewGuid(), userA, quizId, baseTime.AddMinutes(10)),
                QuizAttempt.Start(Guid.NewGuid(), userA, quizId, baseTime.AddMinutes(20)),
                QuizAttempt.Start(Guid.NewGuid(), userA, quizId, baseTime.AddMinutes(30)),
                QuizAttempt.Start(Guid.NewGuid(), userB, quizId, baseTime.AddMinutes(40)),
                QuizAttempt.Start(Guid.NewGuid(), userB, quizId, baseTime.AddMinutes(50)));
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var sut = new QuizRepository(readCtx);

        var firstPage = await sut.GetAttemptsByUserAsync(userA, skip: 0, take: 2);
        var secondPage = await sut.GetAttemptsByUserAsync(userA, skip: 2, take: 2);

        firstPage.Should().HaveCount(2);
        firstPage.Select(a => a.StartedAt).Should().BeInDescendingOrder();
        firstPage[0].StartedAt.Should().Be(baseTime.AddMinutes(30));

        secondPage.Should().HaveCount(1); // userA only has 3 attempts total
        secondPage[0].StartedAt.Should().Be(baseTime.AddMinutes(10));

        firstPage.Concat(secondPage).Should().OnlyContain(a => a.UserId == userA);
    }

    private async Task<Guid> SeedUserAsync()
    {
        var userId = Guid.NewGuid();
        await using var db = CreateDbContext();
        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = $"user-{userId:N}",
            Email = $"user-{userId:N}@test.local",
            NormalizedUserName = $"USER-{userId:N}",
            NormalizedEmail = $"USER-{userId:N}@TEST.LOCAL",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await db.SaveChangesAsync();
        return userId;
    }

    private async Task<(Guid CategoryId, Guid QuizId, Guid[] QuestionIds)> SeedCategoryWithQuizAsync(int questionCount)
    {
        var categoryId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var category = new Category(categoryId, "Test Category", "desc", "icon");
        var questions = Enumerable.Range(0, questionCount).Select(_ => CreateQuestion(categoryId)).ToArray();
        var quiz = Quiz.Create(quizId, categoryId, questions);

        await using var db = CreateDbContext();
        db.Categories.Add(category);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        return (categoryId, quizId, questions.Select(q => q.Id).ToArray());
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
