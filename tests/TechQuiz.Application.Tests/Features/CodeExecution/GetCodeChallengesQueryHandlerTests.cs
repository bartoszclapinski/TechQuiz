using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.CodeExecution;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.CodeExecution;

public class GetCodeChallengesQueryHandlerTests
{
    private readonly ICodeChallengeCatalog _catalog = Substitute.For<ICodeChallengeCatalog>();

    [Fact]
    public async Task Handle_MapsCatalogChallengesToDtos()
    {
        var challenge = CodeChallenge.Create(
            Guid.NewGuid(), Guid.NewGuid(), Difficulty.Medium,
            title: "Reverse a string",
            prompt: "Print the input line reversed.",
            starterCode: "// starter",
            testCases: [new CodeChallengeTestCase("abc", "cba", 0)]);
        _catalog.GetAll().Returns([challenge]);

        var result = await new GetCodeChallengesQueryHandler(_catalog)
            .Handle(new GetCodeChallengesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        var dto = result[0];
        dto.Id.Should().Be(challenge.Id);
        dto.Title.Should().Be("Reverse a string");
        dto.Difficulty.Should().Be("Medium");
        dto.Prompt.Should().Be("Print the input line reversed.");
        dto.StarterCode.Should().Be("// starter");
    }
}
