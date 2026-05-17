using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;

namespace TechQuiz.Infrastructure.Persistence.Seed;

/// <summary>
/// Populates the database with a minimum-viable starting state on a fresh dev environment.
/// Idempotent: returns early if any category already exists, so running the application
/// twice does not duplicate data. Re-seeding requires <c>docker compose down -v</c> + restart.
/// </summary>
public sealed class DataSeeder(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    ILogger<DataSeeder> logger)
{
    public const string DemoUserEmail = "demo@techquiz.local";
    public const string DemoUserName = "demo";
    public const string DemoUserPassword = "Demo123!";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Categories.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed skipped — categories table is not empty");
            return;
        }

        logger.LogInformation("Seeding initial data");

        var unitTesting = new Category(
            id: Guid.NewGuid(),
            name: "Unit Testing",
            description:
                "Unit testing fundamentals in .NET — MSTest, NUnit, xUnit, AAA pattern, " +
                "test doubles, and Moq. Questions sourced from EPAM .NET Fundamentals course (module 003).",
            iconCode: "test-tube");

        var quiz = Quiz.Create(
            id: Guid.NewGuid(),
            categoryId: unitTesting.Id,
            questions:
            [
                // Questions are attached in a follow-up commit; Quiz.Create requires at
                // least one for the invariant, so seed a single placeholder for now to
                // satisfy the Domain. This will be replaced when the real question bank
                // lands in the next commit on this branch.
                BuildPlaceholderQuestion(unitTesting.Id),
            ]);

        db.Categories.Add(unitTesting);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);

        var demoUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = DemoUserName,
            Email = DemoUserEmail,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(demoUser, DemoUserPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"Failed to create demo user: {errors}");
        }

        logger.LogInformation(
            "Seeded category {CategoryName}, quiz {QuizId}, demo user {Email}",
            unitTesting.Name, quiz.Id, demoUser.Email);
    }

    /// <summary>
    /// Placeholder question used only until the real question bank lands in the next
    /// commit on this branch. Removed there.
    /// </summary>
    private static Question BuildPlaceholderQuestion(Guid categoryId)
    {
        var questionId = Guid.NewGuid();
        return Question.Create(
            id: questionId,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Placeholder — replaced by the real question bank in the next commit.",
            explanation: "Placeholder explanation.",
            options:
            [
                new Option(Guid.NewGuid(), questionId, "Yes", isCorrect: true, orderIndex: 0),
                new Option(Guid.NewGuid(), questionId, "No",  isCorrect: false, orderIndex: 1),
            ]);
    }
}
