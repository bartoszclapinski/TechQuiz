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
    public async Task Handle_ReturnsAllCategoriesProjectedToDtos_WithCountAndBestScore()
    {
        var userId = Guid.NewGuid();
        var csharpId = Guid.NewGuid();
        var aspId = Guid.NewGuid();

        _userContext.UserId.Returns(userId);
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Category(csharpId, "C# Basics", "C# fundamentals", "csharp"),
                new Category(aspId,    "ASP.NET",   "ASP.NET Core",    "aspnet"),
            ]);
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [csharpId] = 15, [aspId] = 18 });
        _categoryRepository.GetUserBestScoresAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double> { [csharpId] = 87.5 });

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);

        var csharp = result.Single(c => c.Id == csharpId);
        csharp.Name.Should().Be("C# Basics");
        csharp.QuestionCount.Should().Be(15);
        csharp.UserBestScore.Should().Be(87.5);

        var asp = result.Single(c => c.Id == aspId);
        asp.QuestionCount.Should().Be(18);
        asp.UserBestScore.Should().Be(0d); // never attempted
    }

    [Fact]
    public async Task Handle_NoCategories_ReturnsEmptyList()
    {
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
        var orphanId = Guid.NewGuid();

        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([new Category(orphanId, "Empty Category", "no questions yet", "x")]);
        _categoryRepository.GetQuestionCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>()); // no entry for orphan
        _categoryRepository.GetUserBestScoresAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, double>());

        var result = await CreateSut().Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().ContainSingle()
            .Which.QuestionCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ScopesBestScoresToCurrentUser()
    {
        var userId = Guid.NewGuid();
        _userContext.UserId.Returns(userId);

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
