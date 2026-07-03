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
    internal const string DemoUserPassword = "DemoPass123!";

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

        await SeedCategoryIfMissingAsync(
            name: "SQL",
            description:
                "Relational database and SQL fundamentals — DBMS concepts, keys, constraints, " +
                "relationships, normalization, the SELECT statement, DML/TCL, and DDL. Questions " +
                "sourced from EPAM .NET Fundamentals course (module 011).",
            iconCode: "SQL",
            buildQuestions: SqlQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "EF Core",
            description:
                "Entity Framework Core fundamentals — ORM role, DbContext and DbSet, migrations, " +
                "model configuration (annotations and Fluent API), loading strategies, change " +
                "tracking, and persistence. Questions sourced from EPAM .NET Fundamentals course (module 013).",
            iconCode: "EF",
            buildQuestions: EfCoreQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "ASP.NET Core",
            description:
                "ASP.NET Core fundamentals — app models (MVC, Razor Pages, Blazor), the middleware " +
                "pipeline, dependency injection, routing and route constraints, minimal APIs, Web API " +
                "design, and authentication & authorization. Questions sourced from EPAM .NET " +
                "Fundamentals course (modules 015–021).",
            iconCode: "ASP",
            buildQuestions: AspNetCoreQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "C#/.NET",
            description:
                "C# and .NET runtime fundamentals — reflection and runtime metadata, JSON and XML " +
                "serialization, threads and thread pools, synchronization primitives, the Task Parallel " +
                "Library, and task-based async/await. Questions sourced from EPAM .NET Fundamentals " +
                "course (modules 004–010).",
            iconCode: "C#",
            buildQuestions: CSharpDotNetQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "ADO.NET",
            description:
                "ADO.NET data access fundamentals — the abstract provider model (DbConnection, " +
                "DbCommand, DbDataReader, DbParameter), command execution methods, parameterized queries " +
                "and SQL-injection defence, the connected vs disconnected models (DataSet, DataAdapter), " +
                "transactions, and connection pooling. Questions sourced from EPAM .NET Fundamentals " +
                "course (module 012).",
            iconCode: "ADO",
            buildQuestions: AdoNetQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "Design Patterns",
            description:
                "Software design patterns and principles — the GoF creational, structural, and behavioral " +
                "patterns (Singleton, Factory Method, Builder, Adapter, Decorator, Composite, Strategy, " +
                "Observer, Visitor, and more), the SOLID principles, and architectural styles such as " +
                "Onion/Clean Architecture. Questions cover EPAM .NET Fundamentals course (module 014).",
            iconCode: "GoF",
            buildQuestions: DesignPatternsQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "Front-End",
            description:
                "Front-end web fundamentals — JavaScript and TypeScript (equality and type coercion, scope " +
                "and hoisting, closures, the event loop, promises and async/await, this binding, and the " +
                "TypeScript type system) plus core HTML and CSS (semantic markup, the box model, specificity, " +
                "positioning, flexbox, and grid). Authored content; outside the EPAM .NET Fundamentals track.",
            iconCode: "FE",
            buildQuestions: FrontEndQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(
            name: "Engineering Practices",
            description:
                "Software engineering practices and workflow — Git and version control (staging, branches, " +
                "merge vs rebase, fast-forward, cherry-pick), continuous integration and delivery/deployment " +
                "(pipelines, build-once-deploy-many, feature-branch flow, blue-green), and clean-code " +
                "craftsmanship (meaningful names, DRY, YAGNI, code smells, code review, and the test pyramid). " +
                "Git content draws on EPAM .NET Fundamentals course module 001; the rest is authored.",
            iconCode: "ENG",
            buildQuestions: EngineeringPracticesQuestions.CreateAll,
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
