using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Quizzes;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class StartQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 14, 0, 0, TimeSpan.Zero);

    private StartQuizCommandHandler CreateSut() =>
        new(_quizRepository, _userContext, _unitOfWork, _timeProvider);

    private static Question SampleQuestion(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            qid, categoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "any text", "any explanation",
            [
                new Option(Guid.NewGuid(), qid, "a", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "b", isCorrect: true,  orderIndex: 1),
            ]);
    }

    private static Quiz SampleQuiz(Guid categoryId, int questionCount = 3)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(_ => SampleQuestion(categoryId))
            .ToList();
        return Quiz.Create(Guid.NewGuid(), categoryId, questions);
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsSessionWithAttemptIdAndProjectedQuestions()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quiz = SampleQuiz(categoryId, questionCount: 3);

        _userContext.UserId.Returns(userId);
        _timeProvider.GetUtcNow().Returns(T0);
        _quizRepository.GetByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        var result = await CreateSut().Handle(new StartQuizCommand(categoryId), CancellationToken.None);

        result.AttemptId.Should().NotBeEmpty();
        result.Questions.Should().HaveCount(3);
        // Projection preserves order and ids.
        result.Questions.Select(q => q.Id).Should().Equal(quiz.Questions.Select(q => q.Id));
        // OptionDto has no IsCorrect field — enforced by the type itself.
        result.Questions[0].Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_PersistsAttempt_AndCommitsUnitOfWork()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quiz = SampleQuiz(categoryId);

        _userContext.UserId.Returns(userId);
        _timeProvider.GetUtcNow().Returns(T0);
        _quizRepository.GetByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        await CreateSut().Handle(new StartQuizCommand(categoryId), CancellationToken.None);

        await _quizRepository.Received(1).AddAttemptAsync(
            Arg.Is<QuizAttempt>(a =>
                a.UserId == userId &&
                a.QuizId == quiz.Id &&
                a.StartedAt == T0 &&
                a.CompletedAt == null),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AssignsCurrentUserToAttempt()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quiz = SampleQuiz(categoryId);

        _userContext.UserId.Returns(userId);
        _timeProvider.GetUtcNow().Returns(T0);
        _quizRepository.GetByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        QuizAttempt? captured = null;
        await _quizRepository.AddAttemptAsync(
            Arg.Do<QuizAttempt>(a => captured = a),
            Arg.Any<CancellationToken>());

        await CreateSut().Handle(new StartQuizCommand(categoryId), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_CategoryWithoutQuiz_ThrowsKeyNotFoundException()
    {
        var categoryId = Guid.NewGuid();

        _quizRepository.GetByCategoryAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Quiz?)null);

        var act = async () => await CreateSut().Handle(
            new StartQuizCommand(categoryId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{categoryId}*");

        // Side effects must not happen when lookup fails.
        await _quizRepository.DidNotReceive().AddAttemptAsync(
            Arg.Any<QuizAttempt>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
