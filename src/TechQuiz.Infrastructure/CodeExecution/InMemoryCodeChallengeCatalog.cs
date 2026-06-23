using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.CodeExecution;

/// <summary>
/// Seed source of <see cref="CodeChallenge"/>s held in memory (ADR-018 defers persistence
/// to a later iteration). Registered as a singleton — the seed is immutable and built once.
/// Starter code targets Judge0 language 51 (Mono / C# 8), so challenges use a full
/// <c>Program.Main</c> shell rather than top-level statements.
/// </summary>
public sealed class InMemoryCodeChallengeCatalog : ICodeChallengeCatalog
{
    private const string StarterShell =
        """
        using System;

        class Program
        {
            static void Main()
            {
                // Read input from Console and write your answer to Console.
            }
        }
        """;

    private static readonly Guid CSharpCategoryId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IReadOnlyList<CodeChallenge> _challenges = Build();

    public IReadOnlyList<CodeChallenge> GetAll() => _challenges;

    public CodeChallenge? Find(Guid id) => _challenges.FirstOrDefault(c => c.Id == id);

    private static IReadOnlyList<CodeChallenge> Build() =>
    [
        CodeChallenge.Create(
            Guid.Parse("a0000000-0000-0000-0000-000000000001"),
            CSharpCategoryId,
            Difficulty.Easy,
            title: "Sum two integers",
            prompt: "Read a single line containing two space-separated integers and print their sum.",
            starterCode: StarterShell,
            testCases:
            [
                new CodeChallengeTestCase("2 3", "5", orderIndex: 0),
                new CodeChallengeTestCase("10 20", "30", orderIndex: 1),
                new CodeChallengeTestCase("-4 9", "5", orderIndex: 2),
            ]),

        CodeChallenge.Create(
            Guid.Parse("a0000000-0000-0000-0000-000000000002"),
            CSharpCategoryId,
            Difficulty.Medium,
            title: "FizzBuzz up to N",
            prompt:
                "Read an integer N from a single line. For each i from 1 to N print one line: " +
                "\"Fizz\" if i is divisible by 3, \"Buzz\" if divisible by 5, \"FizzBuzz\" if " +
                "divisible by both, otherwise the number itself.",
            starterCode: StarterShell,
            testCases:
            [
                new CodeChallengeTestCase("5", "1\n2\nFizz\n4\nBuzz", orderIndex: 0),
                new CodeChallengeTestCase("3", "1\n2\nFizz", orderIndex: 1),
                new CodeChallengeTestCase("15",
                    "1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz",
                    orderIndex: 2),
            ]),

        CodeChallenge.Create(
            Guid.Parse("a0000000-0000-0000-0000-000000000003"),
            CSharpCategoryId,
            Difficulty.Hard,
            title: "Balanced brackets",
            prompt:
                "Read a single line containing only the characters ()[]{}. Print \"true\" if the " +
                "brackets are balanced and correctly nested, otherwise print \"false\".",
            starterCode: StarterShell,
            testCases:
            [
                new CodeChallengeTestCase("([])", "true", orderIndex: 0),
                new CodeChallengeTestCase("([)]", "false", orderIndex: 1),
                new CodeChallengeTestCase("(((", "false", orderIndex: 2),
                new CodeChallengeTestCase("{[()]}", "true", orderIndex: 3),
            ]),
    ];
}
