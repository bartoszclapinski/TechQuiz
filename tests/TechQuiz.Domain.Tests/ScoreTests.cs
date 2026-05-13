using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class ScoreTests
{
    private static readonly DateTimeOffset T0 = new(2026, 5, 13, 10, 0, 0, TimeSpan.Zero);

    private sealed record QuestionFixture(Question Question, Guid CorrectOptionId, Guid WrongOptionId);

    private static QuestionFixture MakeQuestion(Difficulty difficulty = Difficulty.Easy)
    {
        var qid = Guid.NewGuid();
        var correctId = Guid.NewGuid();
        var wrongId = Guid.NewGuid();
        var q = Question.Create(
            qid, Guid.NewGuid(), QuestionType.MultipleChoice, difficulty,
            "any text", "any explanation",
            [
                new Option(wrongId,    qid, "wrong",   isCorrect: false, orderIndex: 0),
                new Option(correctId,  qid, "correct", isCorrect: true,  orderIndex: 1),
            ]);
        return new QuestionFixture(q, correctId, wrongId);
    }

    [Fact]
    public void Calculate_AllCorrect_Returns100Percent()
    {
        var fixtures = Enumerable.Range(0, 10).Select(_ => MakeQuestion()).ToList();
        var questions = fixtures.Select(f => f.Question).ToList();
        var answers = fixtures.Select(f => new Answer(f.Question.Id, f.CorrectOptionId, T0)).ToList();

        var score = Score.Calculate(questions, answers);

        score.CorrectCount.Should().Be(10);
        score.TotalCount.Should().Be(10);
        score.Percentage.Should().Be(100d);
    }

    [Fact]
    public void Calculate_AllWrong_Returns0Percent()
    {
        var fixtures = Enumerable.Range(0, 10).Select(_ => MakeQuestion()).ToList();
        var questions = fixtures.Select(f => f.Question).ToList();
        var answers = fixtures.Select(f => new Answer(f.Question.Id, f.WrongOptionId, T0)).ToList();

        var score = Score.Calculate(questions, answers);

        score.CorrectCount.Should().Be(0);
        score.TotalCount.Should().Be(10);
        score.Percentage.Should().Be(0d);
    }

    [Fact]
    public void Calculate_Partial_7Of10_Returns70Percent()
    {
        var fixtures = Enumerable.Range(0, 10).Select(_ => MakeQuestion()).ToList();
        var questions = fixtures.Select(f => f.Question).ToList();
        // First 7 correct, last 3 wrong.
        var answers = fixtures.Select((f, i) =>
            new Answer(f.Question.Id, i < 7 ? f.CorrectOptionId : f.WrongOptionId, T0)).ToList();

        var score = Score.Calculate(questions, answers);

        score.CorrectCount.Should().Be(7);
        score.TotalCount.Should().Be(10);
        score.Percentage.Should().Be(70d);
    }

    [Fact]
    public void Calculate_UnansweredQuestion_CountedAsWrong()
    {
        var f1 = MakeQuestion();
        var f2 = MakeQuestion();

        // Only answer f1; leave f2 entirely missing from the answers list.
        var score = Score.Calculate(
            [f1.Question, f2.Question],
            [new Answer(f1.Question.Id, f1.CorrectOptionId, T0)]);

        score.CorrectCount.Should().Be(1);
        score.TotalCount.Should().Be(2);
        score.Percentage.Should().Be(50d);
    }

    [Fact]
    public void Calculate_NullSelectedOption_CountedAsWrong()
    {
        var f1 = MakeQuestion();

        var score = Score.Calculate(
            [f1.Question],
            [new Answer(f1.Question.Id, selectedOptionId: null, T0)]);

        score.CorrectCount.Should().Be(0);
        score.TotalCount.Should().Be(1);
    }

    [Fact]
    public void Calculate_NoQuestions_Returns0Percent_NoDivisionByZero()
    {
        var score = Score.Calculate([], []);

        score.CorrectCount.Should().Be(0);
        score.TotalCount.Should().Be(0);
        score.Percentage.Should().Be(0d);
    }

    [Fact]
    public void Calculate_DifficultyBreakdown_GroupsCorrectAndTotalPerDifficulty()
    {
        // 3 Easy, 4 Medium, 3 Hard
        var easy = Enumerable.Range(0, 3).Select(_ => MakeQuestion(Difficulty.Easy)).ToList();
        var medium = Enumerable.Range(0, 4).Select(_ => MakeQuestion(Difficulty.Medium)).ToList();
        var hard = Enumerable.Range(0, 3).Select(_ => MakeQuestion(Difficulty.Hard)).ToList();

        var questions = easy.Concat(medium).Concat(hard).Select(f => f.Question).ToList();

        // Easy: all 3 correct; Medium: 2 of 4 correct; Hard: none correct.
        var answers = new List<Answer>();
        answers.AddRange(easy.Select(f => new Answer(f.Question.Id, f.CorrectOptionId, T0)));
        answers.AddRange(medium.Take(2).Select(f => new Answer(f.Question.Id, f.CorrectOptionId, T0)));
        answers.AddRange(medium.Skip(2).Select(f => new Answer(f.Question.Id, f.WrongOptionId, T0)));
        answers.AddRange(hard.Select(f => new Answer(f.Question.Id, f.WrongOptionId, T0)));

        var score = Score.Calculate(questions, answers);

        score.ByDifficulty[Difficulty.Easy].Should().Be((3, 3));
        score.ByDifficulty[Difficulty.Medium].Should().Be((2, 4));
        score.ByDifficulty[Difficulty.Hard].Should().Be((0, 3));
        score.CorrectCount.Should().Be(5);
        score.TotalCount.Should().Be(10);
        score.Percentage.Should().Be(50d);
    }

    [Fact]
    public void Calculate_NullQuestions_Throws()
    {
        var act = () => Score.Calculate(null!, []);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Calculate_NullAnswers_Throws()
    {
        var act = () => Score.Calculate([], null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
