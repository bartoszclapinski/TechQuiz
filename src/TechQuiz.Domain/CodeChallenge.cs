namespace TechQuiz.Domain;

/// <summary>
/// An execution-backed coding task (ADR-018). The user writes C#; the platform compiles
/// and runs it against the hidden <see cref="TestCases"/> in a Judge0 sandbox and grades
/// on observed stdout — not on stored answers or AI judgment. Distinct from
/// <see cref="Question"/>, which is option-based.
/// </summary>
public class CodeChallenge
{
    private readonly List<CodeChallengeTestCase> _testCases = [];

    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public Difficulty Difficulty { get; init; }
    public required string Title { get; init; }
    public required string Prompt { get; init; }
    public string? StarterCode { get; init; }
    public IReadOnlyList<CodeChallengeTestCase> TestCases => _testCases;

    // Parameterless constructor for EF Core materialisation.
    private CodeChallenge() { }

    public static CodeChallenge Create(
        Guid id,
        Guid categoryId,
        Difficulty difficulty,
        string title,
        string prompt,
        string? starterCode,
        IReadOnlyList<CodeChallengeTestCase> testCases)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidCodeChallengeException("CodeChallenge title must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidCodeChallengeException("CodeChallenge prompt must not be empty.");
        }

        if (testCases is null || testCases.Count < 1)
        {
            throw new InvalidCodeChallengeException("CodeChallenge must have at least 1 test case.");
        }

        var challenge = new CodeChallenge
        {
            Id = id,
            CategoryId = categoryId,
            Difficulty = difficulty,
            Title = title,
            Prompt = prompt,
            StarterCode = starterCode,
        };
        challenge._testCases.AddRange(testCases);
        return challenge;
    }
}
