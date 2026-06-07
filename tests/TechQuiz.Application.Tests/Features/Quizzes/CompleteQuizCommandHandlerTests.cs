using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Quizzes;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class CompleteQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 14, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TComplete = T0.AddMinutes(8);

    private CompleteQuizCommandHandler CreateSut() =>
        new(_quizRepository, _userContext, _unitOfWork, _timeProvider);

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

    private Scenario BuildScenario(int questionCount = 3, Difficulty difficulty = Difficulty.Easy)
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var fixtures = Enumerable.Range(0, questionCount)
            .Select(_ => MakeQuestion(categoryId, difficulty))
            .ToList();
        var quiz = Quiz.Create(Guid.NewGuid(), categoryId, fixtures.Select(f => f.Question).ToList());
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quiz.Id, T0);

        _userContext.UserId.Returns(userId);
        _timeProvider.GetUtcNow().Returns(TComplete);
        _quizRepository.GetAttemptAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        _quizRepository.GetByIdAsync(quiz.Id, Arg.Any<CancellationToken>()).Returns(quiz);

        return new Scenario(attempt, quiz, userId, fixtures);
    }

    [Fact]
    public async Task Handle_AllCorrect_ReturnsResultWith100Percent_AndCompletedAttempt()
    {
        var s = BuildScenario(questionCount: 3);
        foreach (var f in s.Fixtures)
        {
            s.Attempt.SubmitAnswer(f.Question.Id, f.CorrectOptionId, T0.AddSeconds(1));
        }

        var result = await CreateSut().Handle(new CompleteQuizCommand(s.Attempt.Id), CancellationToken.None);

        result.AttemptId.Should().Be(s.Attempt.Id);
        result.CorrectCount.Should().Be(3);
        result.TotalCount.Should().Be(3);
        result.Percentage.Should().Be(100d);
        result.CompletedAt.Should().Be(TComplete);
        result.StartedAt.Should().Be(T0);
        result.Questions.Should().HaveCount(3);
        result.Questions.Should().OnlyContain(q => q.IsCorrect);

        s.Attempt.IsCompleted.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Partial_ProjectsPerQuestionCorrectness()
    {
        var s = BuildScenario(questionCount: 3);
        // Q0 correct, Q1 wrong (selected wrong option), Q2 unanswered (no submit).
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));
        s.Attempt.SubmitAnswer(s.Fixtures[1].Question.Id, s.Fixtures[1].WrongOptionId,  T0.AddSeconds(2));

        var result = await CreateSut().Handle(new CompleteQuizCommand(s.Attempt.Id), CancellationToken.None);

        result.CorrectCount.Should().Be(1);
        result.TotalCount.Should().Be(3);

        var q0 = result.Questions.Single(q => q.QuestionId == s.Fixtures[0].Question.Id);
        q0.IsCorrect.Should().BeTrue();
        q0.UserSelectedOptionId.Should().Be(s.Fixtures[0].CorrectOptionId);

        var q1 = result.Questions.Single(q => q.QuestionId == s.Fixtures[1].Question.Id);
        q1.IsCorrect.Should().BeFalse();
        q1.UserSelectedOptionId.Should().Be(s.Fixtures[1].WrongOptionId);

        var q2 = result.Questions.Single(q => q.QuestionId == s.Fixtures[2].Question.Id);
        q2.IsCorrect.Should().BeFalse();
        q2.UserSelectedOptionId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_OptionsExposeIsCorrect_OnResultView()
    {
        var s = BuildScenario(questionCount: 1);
        s.Attempt.SubmitAnswer(s.Fixtures[0].Question.Id, s.Fixtures[0].CorrectOptionId, T0.AddSeconds(1));

        var result = await CreateSut().Handle(new CompleteQuizCommand(s.Attempt.Id), CancellationToken.None);

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
            new CompleteQuizCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttemptBelongsToDifferentUser_Throws_Forbidden()
    {
        var s = BuildScenario();
        _userContext.UserId.Returns(Guid.NewGuid()); // different user

        var act = async () => await CreateSut().Handle(
            new CompleteQuizCommand(s.Attempt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        s.Attempt.IsCompleted.Should().BeFalse();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyCompleted_Throws_QuizAlreadyCompleted()
    {
        var s = BuildScenario();
        s.Attempt.Complete(T0.AddMinutes(1), scorePercentage: 0d);

        var act = async () => await CreateSut().Handle(
            new CompleteQuizCommand(s.Attempt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<QuizAlreadyCompletedException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
