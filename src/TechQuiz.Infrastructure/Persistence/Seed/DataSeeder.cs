using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Seed.Data;

namespace TechQuiz.Infrastructure.Persistence.Seed;

/// <summary>
/// Populates the database with a minimum-viable dev starting state. Each resource has
/// its own existence check, so the seeder is both safely re-runnable (no duplicates) and
/// self-healing across partial failures — if e.g. categories were committed but the demo
/// user wasn't, the next boot creates only the user.
/// </summary>
public sealed class DataSeeder(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    ILogger<DataSeeder> logger)
{
    internal const string DemoUserEmail = "demo@techquiz.local";
    internal const string DemoUserName = "demo";
    internal const string DemoUserPassword = "Demo123!";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCategoryIfMissingAsync(
            name: "Unit Testing",
            description:
                "Unit testing fundamentals in .NET — MSTest, NUnit, xUnit, AAA pattern, " +
                "test doubles, and Moq. Questions sourced from EPAM .NET Fundamentals course (module 003).",
            iconCode: "test-tube",
            buildQuestions: UnitTestingQuestions.CreateAll,
            cancellationToken);

        await SeedDemoUserAsync(cancellationToken);
    }

    private async Task SeedCategoryIfMissingAsync(
        string name,
        string description,
        string iconCode,
        Func<Guid, IReadOnlyList<Question>> buildQuestions,
        CancellationToken cancellationToken)
    {
        if (await db.Categories.AnyAsync(c => c.Name == name, cancellationToken))
        {
            logger.LogInformation("Seed skipped — category {CategoryName} already exists", name);
            return;
        }

        var category = new Category(Guid.NewGuid(), name, description, iconCode);
        var questions = buildQuestions(category.Id);
        var quiz = Quiz.Create(Guid.NewGuid(), category.Id, questions);

        db.Categories.Add(category);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded category {CategoryName} with {QuestionCount} questions, quiz {QuizId}",
            name, questions.Count, quiz.Id);
    }

    private async Task SeedDemoUserAsync(CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(DemoUserEmail) is not null)
        {
            logger.LogInformation("Seed skipped — demo user {Email} already exists", DemoUserEmail);
            return;
        }

        var demoUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = DemoUserName,
            Email = DemoUserEmail,
            EmailConfirmed = true,
        };

        // UserManager.CreateAsync(TUser, string) has no CancellationToken overload —
        // that's an Identity API gap, not something to "complete" higher up the chain.
        // Manual ThrowIfCancellationRequested keeps host-shutdown cooperative even though
        // the call itself can't be cancelled mid-flight.
        cancellationToken.ThrowIfCancellationRequested();
        var result = await userManager.CreateAsync(demoUser, DemoUserPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"Failed to create demo user: {errors}");
        }

        logger.LogInformation("Seeded demo user {Email}", demoUser.Email);
    }
}
