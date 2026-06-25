namespace TechQuiz.Domain;

/// <summary>
/// One hidden grading case for a <see cref="CodeChallenge"/>: the program is run with
/// <see cref="Stdin"/> and its stdout is compared against <see cref="ExpectedStdout"/>.
/// Comparison is trimmed so trailing newlines from <c>Console.WriteLine</c> don't cause
/// spurious failures.
/// </summary>
public sealed class CodeChallengeTestCase
{
    public string Stdin { get; }
    public string ExpectedStdout { get; }
    public int OrderIndex { get; }

    public CodeChallengeTestCase(string stdin, string expectedStdout, int orderIndex)
    {
        Stdin = stdin ?? string.Empty;
        ExpectedStdout = expectedStdout ?? string.Empty;
        OrderIndex = orderIndex;
    }

    public bool Matches(string? actualStdout) =>
        string.Equals(
            (actualStdout ?? string.Empty).Trim(),
            ExpectedStdout.Trim(),
            StringComparison.Ordinal);
}
