using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class CodeChallengeTests
{
    private static readonly Guid AnyChallengeId = Guid.NewGuid();
    private static readonly Guid AnyCategoryId = Guid.NewGuid();

    private static IReadOnlyList<CodeChallengeTestCase> OneValidTestCase() =>
    [
        new CodeChallengeTestCase(stdin: "2 3", expectedStdout: "5", orderIndex: 0),
    ];

    [Fact]
    public void Create_WithValidInput_ReturnsCodeChallenge()
    {
        var challenge = CodeChallenge.Create(
            AnyChallengeId,
            AnyCategoryId,
            Difficulty.Easy,
            title: "Sum two integers",
            prompt: "Read two space-separated integers from stdin and print their sum.",
            starterCode: "// your code here",
            OneValidTestCase());

        challenge.Id.Should().Be(AnyChallengeId);
        challenge.CategoryId.Should().Be(AnyCategoryId);
        challenge.Difficulty.Should().Be(Difficulty.Easy);
        challenge.Title.Should().Be("Sum two integers");
        challenge.Prompt.Should().Be("Read two space-separated integers from stdin and print their sum.");
        challenge.StarterCode.Should().Be("// your code here");
        challenge.TestCases.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithNullStarterCode_IsAllowed()
    {
        var challenge = CodeChallenge.Create(
            AnyChallengeId, AnyCategoryId, Difficulty.Medium,
            "title", "prompt", starterCode: null, OneValidTestCase());

        challenge.StarterCode.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceTitle_Throws(string title)
    {
        var act = () => CodeChallenge.Create(
            AnyChallengeId, AnyCategoryId, Difficulty.Easy,
            title, "prompt", "starter", OneValidTestCase());

        act.Should().Throw<InvalidCodeChallengeException>().WithMessage("*title*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespacePrompt_Throws(string prompt)
    {
        var act = () => CodeChallenge.Create(
            AnyChallengeId, AnyCategoryId, Difficulty.Easy,
            "title", prompt, "starter", OneValidTestCase());

        act.Should().Throw<InvalidCodeChallengeException>().WithMessage("*prompt*");
    }

    [Fact]
    public void Create_WithNoTestCases_Throws()
    {
        var act = () => CodeChallenge.Create(
            AnyChallengeId, AnyCategoryId, Difficulty.Easy,
            "title", "prompt", "starter", []);

        act.Should().Throw<InvalidCodeChallengeException>().WithMessage("*at least 1 test case*");
    }

    [Fact]
    public void Create_WithNullTestCases_Throws()
    {
        var act = () => CodeChallenge.Create(
            AnyChallengeId, AnyCategoryId, Difficulty.Easy,
            "title", "prompt", "starter", testCases: null!);

        act.Should().Throw<InvalidCodeChallengeException>().WithMessage("*at least 1 test case*");
    }

    [Theory]
    [InlineData("5", true)]
    [InlineData("  5  ", true)]
    [InlineData("5\n", true)]
    [InlineData("6", false)]
    [InlineData(null, false)]
    public void TestCase_Matches_ComparesTrimmedStdout(string? actualStdout, bool expectedMatch)
    {
        var testCase = new CodeChallengeTestCase(stdin: "2 3", expectedStdout: "5", orderIndex: 0);

        testCase.Matches(actualStdout).Should().Be(expectedMatch);
    }
}
