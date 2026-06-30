using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;

namespace TechQuiz.Application.Tests.Features.Review;

public class GradeReviewCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();

    private GradeReviewCommandHandler CreateSut() => new(_quizRepository);

    [Fact]
    public async Task Handle_GradesEachAnswer_DerivingCorrectnessAndCarryingExplanation()
    {
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();
        var q3 = Guid.NewGuid();
        var correct1 = Guid.NewGuid();
        var correct2 = Guid.NewGuid();
        var correct3 = Guid.NewGuid();
        _quizRepository.GetQuestionsForGradingByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                new QuestionGradingDto(q1, correct1, "exp1"),
                new QuestionGradingDto(q2, correct2, "exp2"),
                new QuestionGradingDto(q3, correct3, "exp3"),
            ]);

        var command = new GradeReviewCommand([
            new ReviewAnswerInput(q1, correct1),           // correct
            new ReviewAnswerInput(q2, Guid.NewGuid()),     // wrong option
            new ReviewAnswerInput(q3, null),               // skipped
        ]);

        var results = await CreateSut().Handle(command, CancellationToken.None);

        results.Should().HaveCount(3);
        results[0].Should().BeEquivalentTo(new ReviewGradeResultDto(q1, correct1, correct1, true, "exp1"));
        results[1].IsCorrect.Should().BeFalse();
        results[1].CorrectOptionId.Should().Be(correct2);
        results[2].IsCorrect.Should().BeFalse();
        results[2].SelectedOptionId.Should().BeNull();
        results[2].CorrectOptionId.Should().Be(correct3);
    }

    [Fact]
    public async Task Handle_PreservesSubmissionOrder()
    {
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();
        // Repository returns the questions in the opposite order; output must follow the answers.
        _quizRepository.GetQuestionsForGradingByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                new QuestionGradingDto(q2, Guid.NewGuid(), "exp2"),
                new QuestionGradingDto(q1, Guid.NewGuid(), "exp1"),
            ]);

        var command = new GradeReviewCommand([
            new ReviewAnswerInput(q1, Guid.NewGuid()),
            new ReviewAnswerInput(q2, Guid.NewGuid()),
        ]);

        var results = await CreateSut().Handle(command, CancellationToken.None);

        results.Select(r => r.QuestionId).Should().Equal(q1, q2);
    }

    [Fact]
    public async Task Handle_SkipsAnswersForQuestionsMissingFromGrading()
    {
        var known = Guid.NewGuid();
        var unknown = Guid.NewGuid();
        var correct = Guid.NewGuid();
        _quizRepository.GetQuestionsForGradingByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new QuestionGradingDto(known, correct, "exp")]);

        var command = new GradeReviewCommand([
            new ReviewAnswerInput(known, correct),
            new ReviewAnswerInput(unknown, Guid.NewGuid()),
        ]);

        var results = await CreateSut().Handle(command, CancellationToken.None);

        results.Should().ContainSingle().Which.QuestionId.Should().Be(known);
    }

    [Fact]
    public async Task Handle_FetchesGradingForExactlyTheSubmittedQuestionIds()
    {
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();
        _quizRepository.GetQuestionsForGradingByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateSut().Handle(
            new GradeReviewCommand([new ReviewAnswerInput(q1, null), new ReviewAnswerInput(q2, null)]),
            CancellationToken.None);

        await _quizRepository.Received(1).GetQuestionsForGradingByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(q1) && ids.Contains(q2)),
            Arg.Any<CancellationToken>());
    }
}
