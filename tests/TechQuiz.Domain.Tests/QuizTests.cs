using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class QuizTests
{
    private static readonly Guid AnyQuizId = Guid.NewGuid();
    private static readonly Guid AnyCategoryId = Guid.NewGuid();

    private static Question SampleQuestion()
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            qid, AnyCategoryId, QuestionType.MultipleChoice, Difficulty.Easy,
            "any text", "any explanation",
            [
                new Option(Guid.NewGuid(), qid, "a", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "b", isCorrect: true,  orderIndex: 1),
            ]);
    }

    [Fact]
    public void Create_WithValidInput_ReturnsQuiz()
    {
        var questions = new[] { SampleQuestion(), SampleQuestion(), SampleQuestion() };

        var quiz = Quiz.Create(AnyQuizId, AnyCategoryId, questions);

        quiz.Id.Should().Be(AnyQuizId);
        quiz.CategoryId.Should().Be(AnyCategoryId);
        quiz.Questions.Should().HaveCount(3);
    }

    [Fact]
    public void Create_PreservesQuestionOrder()
    {
        var q1 = SampleQuestion();
        var q2 = SampleQuestion();
        var q3 = SampleQuestion();

        var quiz = Quiz.Create(AnyQuizId, AnyCategoryId, [q1, q2, q3]);

        quiz.Questions.Should().Equal(q1, q2, q3);
    }

    [Fact]
    public void Create_WithEmptyQuestions_Throws()
    {
        var act = () => Quiz.Create(AnyQuizId, AnyCategoryId, []);

        act.Should().Throw<ArgumentException>().WithMessage("*at least 1 question*");
    }

    [Fact]
    public void Create_WithNullQuestions_Throws()
    {
        var act = () => Quiz.Create(AnyQuizId, AnyCategoryId, null!);

        act.Should().Throw<ArgumentException>().WithMessage("*at least 1 question*");
    }
}
