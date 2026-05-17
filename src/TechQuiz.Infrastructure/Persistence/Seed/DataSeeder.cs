using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Seed.Data;

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

        var questions = UnitTestingQuestions.CreateAll(unitTesting.Id);
        var quiz = Quiz.Create(
            id: Guid.NewGuid(),
            categoryId: unitTesting.Id,
            questions: questions);

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
            "Seeded category {CategoryName} with {QuestionCount} questions, quiz {QuizId}, demo user {Email}",
            unitTesting.Name, questions.Count, quiz.Id, demoUser.Email);
    }
}
