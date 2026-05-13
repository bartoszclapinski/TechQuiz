using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class QuizAttemptTests
{
    private static readonly Guid AnyAttemptId = Guid.NewGuid();
    private static readonly Guid AnyUserId = Guid.NewGuid();
    private static readonly Guid AnyQuizId = Guid.NewGuid();
    private static readonly Guid AnyQuestionId = Guid.NewGuid();
    private static readonly Guid AnyOptionId = Guid.NewGuid();
    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_ReturnsAttempt_WithStartedAt_AndNoAnswers_AndNotCompleted()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);

        attempt.Id.Should().Be(AnyAttemptId);
        attempt.UserId.Should().Be(AnyUserId);
        attempt.QuizId.Should().Be(AnyQuizId);
        attempt.StartedAt.Should().Be(T0);
        attempt.CompletedAt.Should().BeNull();
        attempt.IsCompleted.Should().BeFalse();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void SubmitAnswer_AddsAnswerToList()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);

        attempt.SubmitAnswer(AnyQuestionId, AnyOptionId, T0.AddMinutes(1));

        attempt.Answers.Should().HaveCount(1);
        attempt.Answers[0].QuestionId.Should().Be(AnyQuestionId);
        attempt.Answers[0].SelectedOptionId.Should().Be(AnyOptionId);
    }

    [Fact]
    public void SubmitAnswer_NullOption_IsAllowed_AsUnanswered()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);

        attempt.SubmitAnswer(AnyQuestionId, selectedOptionId: null, T0.AddMinutes(1));

        attempt.Answers.Should().ContainSingle()
            .Which.SelectedOptionId.Should().BeNull();
    }

    [Fact]
    public void SubmitAnswer_SecondTimeForSameQuestion_ReplacesEarlierAnswer()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);
        var firstChoice = Guid.NewGuid();
        var secondChoice = Guid.NewGuid();

        attempt.SubmitAnswer(AnyQuestionId, firstChoice, T0.AddMinutes(1));
        attempt.SubmitAnswer(AnyQuestionId, secondChoice, T0.AddMinutes(2));

        attempt.Answers.Should().ContainSingle()
            .Which.SelectedOptionId.Should().Be(secondChoice);
    }

    [Fact]
    public void Complete_SetsCompletedAt_AndMarksIsCompleted()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);
        var t1 = T0.AddMinutes(5);

        attempt.Complete(t1);

        attempt.CompletedAt.Should().Be(t1);
        attempt.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Complete_Twice_Throws()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);
        attempt.Complete(T0.AddMinutes(5));

        var act = () => attempt.Complete(T0.AddMinutes(10));

        act.Should().Throw<InvalidOperationException>().WithMessage("*already completed*");
    }

    [Fact]
    public void SubmitAnswer_AfterComplete_Throws()
    {
        var attempt = QuizAttempt.Start(AnyAttemptId, AnyUserId, AnyQuizId, T0);
        attempt.Complete(T0.AddMinutes(5));

        var act = () => attempt.SubmitAnswer(AnyQuestionId, AnyOptionId, T0.AddMinutes(6));

        act.Should().Throw<InvalidOperationException>().WithMessage("*already completed*");
    }
}
