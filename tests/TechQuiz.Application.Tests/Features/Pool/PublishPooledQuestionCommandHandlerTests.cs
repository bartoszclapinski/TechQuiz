using FluentAssertions;
using NSubstitute;
using TechQuiz.Application;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Pool;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Pool;

public class PublishPooledQuestionCommandHandlerTests
{
    private readonly IPooledQuestionRepository _repo = Substitute.For<IPooledQuestionRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Guid _userId = Guid.NewGuid();

    private PublishPooledQuestionCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new PublishPooledQuestionCommandHandler(_repo, _userContext, _unitOfWork);
    }

    private static PooledQuestion DraftOwnedBy(Guid id, Guid owner) =>
        PooledQuestion.Create(
            id, owner, "Anthropic", "topic", new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc),
            QuestionType.MultipleChoice, Difficulty.Easy, "stem", explanation: null,
            [
                new PooledQuestionOption(Guid.NewGuid(), "a", isCorrect: false, orderIndex: 0),
                new PooledQuestionOption(Guid.NewGuid(), "b", isCorrect: true, orderIndex: 1),
            ]);

    [Fact]
    public async Task Handle_OwnDraft_PublishesAndSaves()
    {
        var id = Guid.NewGuid();
        var draft = DraftOwnedBy(id, _userId);
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(draft);

        await CreateSut().Handle(new PublishPooledQuestionCommand(id), CancellationToken.None);

        draft.Status.Should().Be(PooledQuestionStatus.Published);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsKeyNotFound_AndDoesNotSave()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((PooledQuestion?)null);

        var act = () => CreateSut().Handle(new PublishPooledQuestionCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DraftOwnedByAnotherUser_ThrowsForbidden_AndDoesNotPublishOrSave()
    {
        var id = Guid.NewGuid();
        var draft = DraftOwnedBy(id, Guid.NewGuid());
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(draft);

        var act = () => CreateSut().Handle(new PublishPooledQuestionCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        draft.Status.Should().Be(PooledQuestionStatus.Draft);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyPublished_PropagatesDomainException()
    {
        var id = Guid.NewGuid();
        var draft = DraftOwnedBy(id, _userId);
        draft.Publish();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(draft);

        var act = () => CreateSut().Handle(new PublishPooledQuestionCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<PooledQuestionAlreadyPublishedException>();
    }
}
