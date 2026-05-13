using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Quizzes;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Quizzes;

public class SubmitAnswerCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 14, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = T0.AddMinutes(1);

    private SubmitAnswerCommandHandler CreateSut() =>
        new(_quizRepository, _userContext, _unitOfWork, _timeProvider);

    private static Question SampleQuestion(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            qid, categoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "text", "expl",
            [
                new Option(Guid.NewGuid(), qid, "a", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "b", isCorrect: true,  orderIndex: 1),
            ]);
    }

    private record TestScenario(QuizAttempt Attempt, Quiz Quiz, Guid UserId, Question Question);

    private TestScenario BuildScenario()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var question = SampleQuestion(categoryId);
        var quiz = Quiz.Create(Guid.NewGuid(), categoryId, [question]);
        var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quiz.Id, T0);

        _userContext.UserId.Returns(userId);
        _timeProvider.GetUtcNow().Returns(T1);
        _quizRepository.GetAttemptAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        _quizRepository.GetByIdAsync(quiz.Id, Arg.Any<CancellationToken>()).Returns(quiz);

        return new TestScenario(attempt, quiz, userId, question);
    }

    [Fact]
    public async Task Handle_HappyPath_AppendsAnswerToAttempt_AndSaves()
    {
        var s = BuildScenario();
        var selectedOption = s.Question.Options[1];

        await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, s.Question.Id, selectedOption.Id),
            CancellationToken.None);

        s.Attempt.Answers.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                QuestionId = s.Question.Id,
                SelectedOptionId = (Guid?)selectedOption.Id,
                SubmittedAt = T1,
            });

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NullSelectedOption_IsAcceptedAsUnanswered()
    {
        var s = BuildScenario();

        await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, s.Question.Id, SelectedOptionId: null),
            CancellationToken.None);

        s.Attempt.Answers.Should().ContainSingle()
            .Which.SelectedOptionId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AttemptNotFound_Throws_KeyNotFound()
    {
        var attemptId = Guid.NewGuid();
        _quizRepository.GetAttemptAsync(attemptId, Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        var act = async () => await CreateSut().Handle(
            new SubmitAnswerCommand(attemptId, Guid.NewGuid(), null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttemptBelongsToDifferentUser_Throws_Unauthorized()
    {
        var s = BuildScenario();
        _userContext.UserId.Returns(Guid.NewGuid()); // different user

        var act = async () => await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, s.Question.Id, s.Question.Options[0].Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CompletedAttempt_Throws_QuizAlreadyCompleted()
    {
        var s = BuildScenario();
        s.Attempt.Complete(T1);

        var act = async () => await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, s.Question.Id, s.Question.Options[0].Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<QuizAlreadyCompletedException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuestionNotInQuiz_Throws_ArgumentException()
    {
        var s = BuildScenario();
        var unrelatedQuestionId = Guid.NewGuid();

        var act = async () => await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, unrelatedQuestionId, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{unrelatedQuestionId}*");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MissingQuizForAttempt_Throws_InvalidOperation()
    {
        var s = BuildScenario();
        _quizRepository.GetByIdAsync(s.Quiz.Id, Arg.Any<CancellationToken>())
            .Returns((Quiz?)null);

        var act = async () => await CreateSut().Handle(
            new SubmitAnswerCommand(s.Attempt.Id, s.Question.Id, s.Question.Options[0].Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
