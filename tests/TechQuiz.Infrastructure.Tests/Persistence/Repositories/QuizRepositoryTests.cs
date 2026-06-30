using FluentAssertions;
using TechQuiz.Application.Features.History;
using TechQuiz.Domain;
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
        var userId = await CreateUserAsync();
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
        var userId = await CreateUserAsync();
        var (_, quizId, questions) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var question = questions[0];
        var selectedOptionId = question.Options[0].Id;

        var attemptId = Guid.NewGuid();
        var attempt = QuizAttempt.Start(attemptId, userId, quizId, DateTimeOffset.UtcNow);
        attempt.SubmitAnswer(question.Id, selectedOptionId, submittedAt: DateTimeOffset.UtcNow);

        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.Add(attempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = CreateDbContext();
        var roundTripped = await new QuizRepository(readCtx).GetAttemptAsync(attemptId);

        roundTripped.Should().NotBeNull();
        roundTripped!.Answers.Should().HaveCount(1);
        roundTripped.Answers[0].QuestionId.Should().Be(question.Id);
        roundTripped.Answers[0].SelectedOptionId.Should().Be(selectedOptionId);
    }

    [Fact]
    public async Task GetAttemptsByUserAsync_ScopesToUser_OrdersDescByStartedAt_AndPaginates()
    {
        var userA = await CreateUserAsync();
        var userB = await CreateUserAsync();
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

        secondPage.Should().HaveCount(1);
        secondPage[0].StartedAt.Should().Be(baseTime.AddMinutes(10));

        firstPage.Concat(secondPage).Should().OnlyContain(a => a.UserId == userA);
    }

    [Fact]
    public async Task GetLastCompletedScoreAsync_ReturnsMostRecentPriorScore_ExcludingGivenAttempt()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 30d);
        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 70d);
        var current = await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(20), t.AddMinutes(25), scorePercentage: 90d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetLastCompletedScoreAsync(user, quizId, excludeAttemptId: current);

        result.Should().Be(70d);
    }

    [Fact]
    public async Task GetLastCompletedScoreAsync_ReturnsNull_WhenNoEarlierCompletedAttempt()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var current = await SeedCompletedAttemptAsync(
            user, quizId, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, scorePercentage: 80d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetLastCompletedScoreAsync(user, quizId, excludeAttemptId: current);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLastCompletedScoreAsync_IgnoresInProgressAttempts_AndOtherQuizzes()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var (_, otherQuizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedInProgressAttemptAsync(user, quizId, t.AddMinutes(30));
        await SeedCompletedAttemptAsync(user, otherQuizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 99d);
        var current = await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(40), t.AddMinutes(45), scorePercentage: 50d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetLastCompletedScoreAsync(user, quizId, excludeAttemptId: current);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCompletedAttemptsWithCategoryAsync_ReturnsScopedCompletedAttempts_WithCategoryName_OrderedByCompletedAt()
    {
        var userA = await CreateUserAsync();
        var userB = await CreateUserAsync();
        var (categoryId, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var expectedCategory = $"Test Category {categoryId}";
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(userA, quizId, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 80d);
        await SeedCompletedAttemptAsync(userA, quizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 40d);
        await SeedInProgressAttemptAsync(userA, quizId, t.AddMinutes(30));
        await SeedCompletedAttemptAsync(userB, quizId, t.AddMinutes(20), t.AddMinutes(25), scorePercentage: 99d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var rows = await sut.GetCompletedAttemptsWithCategoryAsync(userA);

        rows.Should().HaveCount(2);
        rows.Select(r => r.ScorePercentage).Should().Equal(40d, 80d);
        rows.Select(r => r.CompletedAt).Should().BeInAscendingOrder();
        rows.Should().OnlyContain(r => r.Category == expectedCategory);
    }

    [Fact]
    public async Task GetCompletedAttemptsWithCategoryAsync_ProjectsAnswerCount()
    {
        var user = await CreateUserAsync();
        var (_, quizId, questions) = await SeedCategoryWithQuizAsync(questionCount: 2);

        var attempt = QuizAttempt.Start(Guid.NewGuid(), user, quizId, DateTimeOffset.UtcNow.AddMinutes(-5));
        attempt.SubmitAnswer(questions[0].Id, questions[0].Options[0].Id, DateTimeOffset.UtcNow.AddMinutes(-4));
        attempt.SubmitAnswer(questions[1].Id, questions[1].Options[0].Id, DateTimeOffset.UtcNow.AddMinutes(-3));
        attempt.Complete(DateTimeOffset.UtcNow, scorePercentage: 100d);
        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.Add(attempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var rows = await sut.GetCompletedAttemptsWithCategoryAsync(user);

        rows.Should().ContainSingle().Which.AnswerCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCompletedHistoryPageAsync_ReturnsScopedCompletedScored_WithCategoryName()
    {
        var userA = await CreateUserAsync();
        var userB = await CreateUserAsync();
        var (categoryId, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var expectedCategory = $"Test Category {categoryId}";
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(userA, quizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 40d);
        await SeedCompletedAttemptAsync(userA, quizId, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 80d);
        await SeedInProgressAttemptAsync(userA, quizId, t.AddMinutes(30));
        await SeedCompletedAttemptAsync(userB, quizId, t.AddMinutes(20), t.AddMinutes(25), scorePercentage: 99d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var rows = await sut.GetCompletedHistoryPageAsync(
            userA, category: null, HistorySortField.Date, descending: true, skip: 0, take: 20);

        rows.Should().HaveCount(2);
        rows.Should().OnlyContain(r => r.Category == expectedCategory);
    }

    [Fact]
    public async Task GetCompletedHistoryPageAsync_SortByDate_HonorsDirection()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 40d);
        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 60d);
        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(20), t.AddMinutes(25), scorePercentage: 80d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var desc = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Date, descending: true, skip: 0, take: 20);
        var asc = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Date, descending: false, skip: 0, take: 20);

        desc.Select(r => r.CompletedAt).Should().BeInDescendingOrder();
        asc.Select(r => r.CompletedAt).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCompletedHistoryPageAsync_SortByScore_HonorsDirection()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 50d);
        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 90d);
        await SeedCompletedAttemptAsync(user, quizId, t.AddMinutes(20), t.AddMinutes(25), scorePercentage: 20d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var desc = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Score, descending: true, skip: 0, take: 20);
        var asc = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Score, descending: false, skip: 0, take: 20);

        desc.Select(r => r.ScorePercentage).Should().Equal(90d, 50d, 20d);
        asc.Select(r => r.ScorePercentage).Should().Equal(20d, 50d, 90d);
    }

    [Fact]
    public async Task GetCompletedHistoryPageAsync_FiltersByCategory()
    {
        var user = await CreateUserAsync();
        var (catA, quizA, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var (_, quizB, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var categoryA = $"Test Category {catA}";
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedCompletedAttemptAsync(user, quizA, t.AddMinutes(0), t.AddMinutes(5), scorePercentage: 40d);
        await SeedCompletedAttemptAsync(user, quizB, t.AddMinutes(10), t.AddMinutes(15), scorePercentage: 80d);

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var rows = await sut.GetCompletedHistoryPageAsync(
            user, category: categoryA, HistorySortField.Date, descending: true, skip: 0, take: 20);

        rows.Should().ContainSingle().Which.Category.Should().Be(categoryA);
    }

    [Fact]
    public async Task GetCompletedHistoryPageAsync_Paginates_ViaSkipTake()
    {
        var user = await CreateUserAsync();
        var (_, quizId, _) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < 5; i++)
        {
            await SeedCompletedAttemptAsync(
                user, quizId, t.AddMinutes(i * 10), t.AddMinutes(i * 10 + 5), scorePercentage: 10d * i);
        }

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        // Date desc: newest first. Page 1 = newest 2, page 2 = next 2.
        var firstPage = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Date, descending: true, skip: 0, take: 2);
        var secondPage = await sut.GetCompletedHistoryPageAsync(
            user, category: null, HistorySortField.Date, descending: true, skip: 2, take: 2);

        firstPage.Should().HaveCount(2);
        secondPage.Should().HaveCount(2);
        firstPage[0].CompletedAt.Should().Be(t.AddMinutes(45));
        firstPage.Select(r => r.CompletedAt).Should().BeInDescendingOrder();
        secondPage.Select(r => r.CompletedAt).Should().BeInDescendingOrder();
        firstPage.Select(r => r.AttemptId).Should().NotIntersectWith(secondPage.Select(r => r.AttemptId));
    }

    [Fact]
    public async Task GetReviewCandidatesAsync_DerivesWasCorrect_FromSelectedOption_AndCarriesDifficulty()
    {
        var user = await CreateUserAsync();
        var (_, quizId, questions) = await SeedCategoryWithQuizAsync(questionCount: 3);
        var correct = questions[0];
        var wrong = questions[1];
        var unanswered = questions[2];
        // CreateQuestion seeds option[0] correct, option[1] wrong.
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var attempt = QuizAttempt.Start(Guid.NewGuid(), user, quizId, t);
        attempt.SubmitAnswer(correct.Id, correct.Options[0].Id, t.AddMinutes(1));
        attempt.SubmitAnswer(wrong.Id, wrong.Options[1].Id, t.AddMinutes(2));
        attempt.SubmitAnswer(unanswered.Id, null, t.AddMinutes(3));
        attempt.Complete(t.AddMinutes(4), scorePercentage: 33d);
        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.Add(attempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var candidates = await sut.GetReviewCandidatesAsync(user);

        candidates.Should().HaveCount(3);
        candidates.Single(c => c.QuestionId == correct.Id).WasCorrect.Should().BeTrue();
        candidates.Single(c => c.QuestionId == wrong.Id).WasCorrect.Should().BeFalse();
        candidates.Single(c => c.QuestionId == unanswered.Id).WasCorrect.Should().BeFalse();
        candidates.Should().OnlyContain(c => c.Difficulty == Difficulty.Easy);
        candidates.Single(c => c.QuestionId == wrong.Id).LastAnsweredAt.Should().Be(t.AddMinutes(2));
    }

    [Fact]
    public async Task GetReviewCandidatesAsync_ScopesToUser_AndExcludesInProgressAttempts()
    {
        var user = await CreateUserAsync();
        var other = await CreateUserAsync();
        var (_, quizId, questions) = await SeedCategoryWithQuizAsync(questionCount: 1);
        var q = questions[0];
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var inProgress = QuizAttempt.Start(Guid.NewGuid(), user, quizId, t);
        inProgress.SubmitAnswer(q.Id, q.Options[1].Id, t.AddMinutes(1));

        var otherUserAttempt = QuizAttempt.Start(Guid.NewGuid(), other, quizId, t.AddMinutes(2));
        otherUserAttempt.SubmitAnswer(q.Id, q.Options[1].Id, t.AddMinutes(3));
        otherUserAttempt.Complete(t.AddMinutes(4), scorePercentage: 0d);

        await using (var writeCtx = CreateDbContext())
        {
            writeCtx.QuizAttempts.AddRange(inProgress, otherUserAttempt);
            await writeCtx.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var candidates = await sut.GetReviewCandidatesAsync(user);

        candidates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReviewQuestionsByIdsAsync_ReturnsRequested_WithCategory_AndOrderedOptions()
    {
        var (categoryId, _, questions) = await SeedCategoryWithQuizAsync(questionCount: 3);
        var expectedCategory = $"Test Category {categoryId}";
        var wanted = new[] { questions[0].Id, questions[2].Id };

        await using var db = CreateDbContext();
        var sut = new QuizRepository(db);

        var result = await sut.GetReviewQuestionsByIdsAsync(wanted);

        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().BeEquivalentTo(wanted);
        result.Should().OnlyContain(r => r.Category == expectedCategory);
        var first = result.Single(r => r.Id == questions[0].Id);
        first.Options.Should().HaveCount(2);
        first.Options.Select(o => o.OrderIndex).Should().BeInAscendingOrder();
    }

    private async Task<Guid> SeedCompletedAttemptAsync(
        Guid userId, Guid quizId, DateTimeOffset startedAt, DateTimeOffset completedAt, double scorePercentage)
    {
        var attemptId = Guid.NewGuid();
        var attempt = QuizAttempt.Start(attemptId, userId, quizId, startedAt);
        attempt.Complete(completedAt, scorePercentage);
        await using var db = CreateDbContext();
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return attemptId;
    }

    private async Task SeedInProgressAttemptAsync(Guid userId, Guid quizId, DateTimeOffset startedAt)
    {
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quizId, startedAt);
        await using var db = CreateDbContext();
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();
    }

    private async Task<(Guid CategoryId, Guid QuizId, IReadOnlyList<Question> Questions)> SeedCategoryWithQuizAsync(int questionCount)
    {
        var categoryId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var category = new Category(categoryId, $"Test Category {categoryId}", "desc", "icon");
        var questions = Enumerable.Range(0, questionCount).Select(_ => CreateQuestion(categoryId)).ToArray();
        var quiz = Quiz.Create(quizId, categoryId, questions);

        await using var db = CreateDbContext();
        db.Categories.Add(category);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        return (categoryId, quizId, questions);
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
