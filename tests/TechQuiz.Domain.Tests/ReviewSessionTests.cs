using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class ReviewSessionTests
{
    private static readonly Guid AnySessionId = Guid.NewGuid();
    private static readonly Guid AnyUserId = Guid.NewGuid();
    private static readonly DateTimeOffset T0 = new(2026, 6, 30, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ReturnsSession_WithItems_AndCompletedAt()
    {
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();
        var o1 = Guid.NewGuid();

        var session = ReviewSession.Create(
            AnySessionId, AnyUserId, T0,
            [new ReviewItem(q1, o1), new ReviewItem(q2, selectedOptionId: null)]);

        session.Id.Should().Be(AnySessionId);
        session.UserId.Should().Be(AnyUserId);
        session.CompletedAt.Should().Be(T0);
        session.QuestionCount.Should().Be(2);
        session.Items.Should().HaveCount(2);
        session.Items[0].QuestionId.Should().Be(q1);
        session.Items[0].SelectedOptionId.Should().Be(o1);
        session.Items[1].SelectedOptionId.Should().BeNull();
    }

    [Fact]
    public void Create_NoItems_Throws()
    {
        var act = () => ReviewSession.Create(AnySessionId, AnyUserId, T0, []);

        act.Should().Throw<InvalidReviewSessionException>().WithMessage("*at least one*");
    }

    [Fact]
    public void Create_NullItems_Throws()
    {
        var act = () => ReviewSession.Create(AnySessionId, AnyUserId, T0, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
