using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Quizzes;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class GetQuizResultQueryHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 14, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TComplete = T0.AddMinutes(8);

    private GetQuizResultQueryHandler CreateSut() => new(_quizRepository, _categoryRepository, _userContext);

    private sealed record QuestionFixture(Question Question, Guid CorrectOptionId, Guid WrongOptionId);

    private static QuestionFixture MakeQuestion(Guid categoryId, Difficulty difficulty = Difficulty.Easy)
    {
        var qid = Guid.NewGuid();
        var correctId = Guid.NewGuid();
        var wrongId = Guid.NewGuid();
        var q = Question.Create(
            qid, categoryId, QuestionType.MultipleChoice, difficulty,
            "any text", "any explanation",
            [
                new Option(wrongId,   qid, "wrong",   isCorrect: false, orderIndex: 0),
                new Option(correctId, qid, "correct", isCorrect: true,  orderIndex: 1),
            ]);
        return new QuestionFixture(q, correctId, wrongId);
    }

    private sealed record Scenario(QuizAttempt Attempt, Quiz Quiz, Guid UserId, IReadOnlyList<QuestionFixture> Fixtures);

    private Scenario BuildScenario(int questionCount = 3)
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fixtures = Enumerable.Range(0, questionCount)
            .Select(_ => MakeQuestion(categoryId))
            .ToList();
        var quiz = Quiz.Create(Guid.NewGuid(), categoryId, fixtures.Select(f => f.Question).ToList());
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quiz.Id, T0);

        _userContext.UserId.Returns(userId);
        _quizRepository.GetAttemptAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        _quizRepository.GetByIdAsync(quiz.Id, Arg.Any<CancellationToken>()).Returns(quiz);

        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category> { new(categoryId, "C# Basics", "desc", "icon") });
        _categoryRepository.GetUserBestScoresAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        return new Scenario(attempt, quiz, userId, fixtures);
    }

    [Fact]
    public async Task Handle_CompletedAttempt_ReturnsResultWithoutMutating()
    {
        var s = BuildScenario(questionCount: 3);
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));
        s.Attempt.SubmitAnswer(s.Fixtures[1].Question.Id, s.Fixtures[1].WrongOptionId,  T0.AddSeconds(2));
        s.Attempt.Complete(TComplete, scorePercentage: 0d);

        var result = await CreateSut().Handle(new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        result.AttemptId.Should().Be(s.Attempt.Id);
        result.CorrectCount.Should().Be(1);
        result.TotalCount.Should().Be(3);
        result.StartedAt.Should().Be(T0);
        result.CompletedAt.Should().Be(TComplete);
        result.Questions.Should().HaveCount(3);

        // A read-only query: it must not re-complete or otherwise touch persistence.
        s.Attempt.CompletedAt.Should().Be(TComplete);
    }

    [Fact]
    public async Task Handle_EnrichesResult_WithCategoryName_BestAndPreviousScore()
    {
        var s = BuildScenario(questionCount: 4);
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));
        s.Attempt.Complete(TComplete, scorePercentage: 25d);

        _categoryRepository.GetUserBestScoresAsync(s.UserId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double> { [s.Quiz.CategoryId] = 95d });
        _quizRepository.GetLastCompletedScoreAsync(s.UserId, s.Quiz.Id, s.Attempt.Id, Arg.Any<CancellationToken>())
            .Returns(60d);

        var result = await CreateSut().Handle(new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        result.CategoryId.Should().Be(s.Quiz.CategoryId);
        result.CategoryName.Should().Be("C# Basics");
        result.BestPercentage.Should().Be(95d);
        result.PreviousPercentage.Should().Be(60d);
    }

    [Fact]
    public async Task Handle_FirstAttempt_HasNullPreviousScore_AndBestFallsBackToCurrent()
    {
        var s = BuildScenario(questionCount: 4);
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));
        s.Attempt.Complete(TComplete, scorePercentage: 25d);
        // No best-score entry for this category and no prior completed attempt.

        var result = await CreateSut().Handle(new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        result.PreviousPercentage.Should().BeNull();
        result.BestPercentage.Should().Be(result.Percentage);
    }

    [Fact]
    public async Task Handle_ResultViewExposesIsCorrectOnOptions()
    {
        var s = BuildScenario(questionCount: 1);
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));
        s.Attempt.Complete(TComplete, scorePercentage: 0d);

        var result = await CreateSut().Handle(new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        var question = result.Questions.Single();
        question.Options.Should().HaveCount(2);
        question.Options.Should().ContainSingle(o => o.IsCorrect);
    }

    [Fact]
    public async Task Handle_AttemptNotFound_Throws_KeyNotFound()
    {
        _quizRepository.GetAttemptAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        var act = async () => await CreateSut().Handle(
            new GetQuizResultQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_AttemptBelongsToDifferentUser_Throws_Forbidden()
    {
        var s = BuildScenario();
        s.Attempt.Complete(TComplete, scorePercentage: 0d);
        _userContext.UserId.Returns(Guid.NewGuid()); // different user

        var act = async () => await CreateSut().Handle(
            new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_AttemptNotCompleted_Throws_QuizNotCompleted()
    {
        var s = BuildScenario();
        // No Complete() call — result is not available yet.

        var act = async () => await CreateSut().Handle(
            new GetQuizResultQuery(s.Attempt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<QuizNotCompletedException>();
    }
}
