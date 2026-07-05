using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Categories;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Categories;

public class GetCategoriesQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    private GetCategoriesQueryHandler CreateSut() => new(_categoryRepository, _userContext);

    [Fact]
    public async Task Handle_GroupsCategoriesUnderTracks_WithCountAndBestScore()
    {
        var userId = Guid.NewGuid();
        var dotnetId = Guid.NewGuid();
        var csharpId = Guid.NewGuid();
        var aspId = Guid.NewGuid();

        _userContext.UserId.Returns(userId);
        _categoryRepository.GetTracksAsync(Arg.Any<CancellationToken>())
            .Returns([new Track(dotnetId, ".NET", "The .NET platform", ".NET", position: 0)]);
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Category(csharpId, dotnetId, "C#/.NET", "C# fundamentals", "C#", position: 0),
                new Category(aspId, dotnetId, "ASP.NET Core", "ASP.NET Core", "ASP", position: 1),
            ]);
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [csharpId] = 15, [aspId] = 18 });
        _categoryRepository.GetUserBestScoresAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double> { [csharpId] = 87.5 });

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        var track = result.Single();
        track.Id.Should().Be(dotnetId);
        track.Name.Should().Be(".NET");
        track.Categories.Should().HaveCount(2);

        var csharp = track.Categories.Single(c => c.Id == csharpId);
        csharp.Name.Should().Be("C#/.NET");
        csharp.QuestionCount.Should().Be(15);
        csharp.UserBestScore.Should().Be(87.5);

        var asp = track.Categories.Single(c => c.Id == aspId);
        asp.QuestionCount.Should().Be(18);
        asp.UserBestScore.Should().Be(0d); // never attempted
    }

    [Fact]
    public async Task Handle_OrdersTracksAndCategoriesByPosition()
    {
        var trackA = Guid.NewGuid();
        var trackB = Guid.NewGuid();
        var catFirst = Guid.NewGuid();
        var catSecond = Guid.NewGuid();

        _categoryRepository.GetTracksAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Track(trackB, "Databases", "SQL", "DB", position: 1),
                new Track(trackA, ".NET", "The .NET platform", ".NET", position: 0),
            ]);
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Category(catSecond, trackA, "ASP.NET Core", "asp", "ASP", position: 1),
                new Category(catFirst, trackA, "C#/.NET", "csharp", "C#", position: 0),
            ]);
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _categoryRepository.GetUserBestScoresAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Select(t => t.Name).Should().ContainInOrder(".NET", "Databases");
        result.First().Categories.Select(c => c.Id).Should().ContainInOrder(catFirst, catSecond);
    }

    [Fact]
    public async Task Handle_NoTracks_ReturnsEmptyList()
    {
        _categoryRepository.GetTracksAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Track>());
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Category>());
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _categoryRepository.GetUserBestScoresAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CategoryWithoutQuestions_ReturnsZeroCount()
    {
        var trackId = Guid.NewGuid();
        var orphanId = Guid.NewGuid();

        _categoryRepository.GetTracksAsync(Arg.Any<CancellationToken>())
            .Returns([new Track(trackId, ".NET", "The .NET platform", ".NET", position: 0)]);
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([new Category(orphanId, trackId, "Empty Category", "no questions yet", "x", position: 0)]);
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>()); // no entry for orphan
        _categoryRepository.GetUserBestScoresAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().ContainSingle()
            .Which.Categories.Should().ContainSingle()
            .Which.QuestionCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ScopesBestScoresToCurrentUser()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);

        _categoryRepository.GetTracksAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Track>());
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Category>());
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _categoryRepository.GetUserBestScoresAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        await _categoryRepository.Received(1)
            .GetUserBestScoresAsync(userId, Arg.Any<CancellationToken>());
    }
}
