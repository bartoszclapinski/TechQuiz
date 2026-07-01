using FluentAssertions;
using NSubstitute;
using TechQuiz.Application;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.Review;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Review;

public class GetReviewSessionDetailQueryHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SessionId = Guid.NewGuid();
    private static readonly DateTimeOffset CompletedAt = new(2026, 7, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    public GetReviewSessionDetailQueryHandlerTests()
    {
        _userContext.UserId.Returns(UserId);
    }

    private GetReviewSessionDetailQueryHandler CreateSut() => new(_quizRepository, _userContext);

    private void GivenSession(ReviewSessionDetailResult? result) =>
        _quizRepository.GetReviewSessionDetailAsync(SessionId, Arg.Any<CancellationToken>())
            .Returns(result);

    private static ReviewSessionDetailResult DetailOwnedBy(Guid ownerId)
    {
        var correct = Guid.NewGuid();
        var items = new[]
        {
            new ReviewSessionItemDto(
                Guid.NewGuid(),
                "What is EF Core?",
                "EF Core",
                Difficulty.Easy,
                [new OptionDto(correct, "An ORM", 0), new OptionDto(Guid.NewGuid(), "A database", 1)],
                correct,
                correct,
                true,
                "EF Core is an object-relational mapper."),
        };
        return new ReviewSessionDetailResult(SessionId, ownerId, CompletedAt, items);
    }

    [Fact]
    public async Task Handle_SessionMissing_ThrowsKeyNotFound()
    {
        GivenSession(null);

        var act = () => CreateSut().Handle(new GetReviewSessionDetailQuery(SessionId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_SessionOwnedByAnotherUser_ThrowsForbidden()
    {
        GivenSession(DetailOwnedBy(Guid.NewGuid()));

        var act = () => CreateSut().Handle(new GetReviewSessionDetailQuery(SessionId), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_OwnedSession_ReturnsDetail_WithoutOwnerId()
    {
        var detail = DetailOwnedBy(UserId);
        GivenSession(detail);

        var result = await CreateSut().Handle(new GetReviewSessionDetailQuery(SessionId), CancellationToken.None);

        result.Id.Should().Be(SessionId);
        result.CompletedAt.Should().Be(CompletedAt);
        result.Items.Should().BeEquivalentTo(detail.Items);
    }
}
