using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;
using TechQuiz.Application.Features.History;

namespace TechQuiz.Application.Tests.Features.History;

public class GetHistoryQueryHandlerTests
{
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    private GetHistoryQueryHandler CreateSut() => new(_quizRepository, _userContext);

    [Fact]
    public async Task Handle_PassesAllParams_AndComputesSkipFromPage()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetCompletedHistoryPageAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<HistorySortField>(),
                Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<HistoryItemDto>());

        // Page 3, PageSize 10 → skip 20, take 10.
        await CreateSut().Handle(
            new GetHistoryQuery(Category: "C#", SortBy: HistorySortField.Score, Descending: false, Page: 3, PageSize: 10),
            CancellationToken.None);

        await _quizRepository.Received(1).GetCompletedHistoryPageAsync(
            userId, "C#", HistorySortField.Score, false, 20, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ScopesToCurrentUser()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);
        _quizRepository.GetCompletedHistoryPageAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<HistorySortField>(),
                Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<HistoryItemDto>());

        await CreateSut().Handle(new GetHistoryQuery(), CancellationToken.None);

        await _quizRepository.Received(1).GetCompletedHistoryPageAsync(
            userId, Arg.Any<string?>(), Arg.Any<HistorySortField>(),
            Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsRepositoryItems_Unchanged()
    {
        _userContext.UserId.Returns(Guid.NewGuid());
        var items = new[]
        {
            new HistoryItemDto(Guid.NewGuid(), "C#", 80d, new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero)),
        };
        _quizRepository.GetCompletedHistoryPageAsync(
                Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<HistorySortField>(),
                Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await CreateSut().Handle(new GetHistoryQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(items);
    }
}
