using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechQuiz.Domain;
using TechQuiz.Infrastructure.Persistence.Identity;
using TechQuiz.Infrastructure.Persistence.Seed.Data;

namespace TechQuiz.Infrastructure.Persistence.Seed;

/// <summary>
/// Populates the database with a minimum-viable starting state: the Track → Category → Quiz
/// taxonomy (ADR-023) plus a demo user. Each resource has its own existence check, so the seeder
/// is both safely re-runnable (no duplicates) and self-healing across partial failures.
/// The one exception is the demo user's quiz history: it is <em>refreshed</em> on every run (wiped
/// and regenerated with dates relative to now) so the demo dashboard always reads "live" rather than
/// going stale as time passes.
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
        await SeedDotNetTrackAsync(cancellationToken);
        await SeedDatabasesTrackAsync(cancellationToken);
        await SeedFrontEndTrackAsync(cancellationToken);
        await SeedEngineeringTrackAsync(cancellationToken);
        await SeedDemoUserAsync(cancellationToken);
        await SeedDemoHistoryAsync(cancellationToken);
    }

    private async Task SeedDotNetTrackAsync(CancellationToken cancellationToken)
    {
        var track = await SeedTrackAsync(
            name: ".NET",
            description:
                "The .NET platform end to end — the C# language and runtime, ASP.NET Core web apps and " +
                "APIs, data access with EF Core and ADO.NET, unit testing, and design patterns.",
            iconCode: ".NET",
            position: 0,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 0,
            name: "C#/.NET",
            description:
                "C# and .NET runtime fundamentals — reflection and runtime metadata, JSON and XML " +
                "serialization, threads and thread pools, synchronization primitives, the Task Parallel " +
                "Library, and task-based async/await.",
            iconCode: "C#",
            buildQuestions: CSharpDotNetQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 1,
            name: "ASP.NET Core",
            description:
                "ASP.NET Core fundamentals — app models (MVC, Razor Pages, Blazor), the middleware " +
                "pipeline, dependency injection, routing and route constraints, minimal APIs, Web API " +
                "design, and authentication & authorization.",
            iconCode: "ASP",
            buildQuestions: AspNetCoreQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 2,
            name: "EF Core",
            description:
                "Entity Framework Core fundamentals — the ORM role, DbContext and DbSet, migrations, " +
                "model configuration (annotations and Fluent API), loading strategies, change tracking, " +
                "and persistence.",
            iconCode: "EF",
            buildQuestions: EfCoreQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 3,
            name: "ADO.NET",
            description:
                "ADO.NET data access fundamentals — the abstract provider model (DbConnection, DbCommand, " +
                "DbDataReader, DbParameter), command execution methods, parameterized queries and " +
                "SQL-injection defence, the connected vs disconnected models, transactions, and connection pooling.",
            iconCode: "ADO",
            buildQuestions: AdoNetQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 4,
            name: "Unit Testing",
            description:
                "Unit testing fundamentals in .NET — MSTest, NUnit, and xUnit, the AAA pattern, test " +
                "doubles, and Moq.",
            iconCode: "test-tube",
            buildQuestions: UnitTestingQuestions.CreateAll,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 5,
            name: "Design Patterns",
            description:
                "Software design patterns and principles — the GoF creational, structural, and behavioral " +
                "patterns (Singleton, Factory Method, Builder, Adapter, Decorator, Composite, Strategy, " +
                "Observer, Visitor, and more), the SOLID principles, and architectural styles such as " +
                "Onion/Clean Architecture.",
            iconCode: "GoF",
            buildQuestions: DesignPatternsQuestions.CreateAll,
            cancellationToken);
    }

    private async Task SeedDatabasesTrackAsync(CancellationToken cancellationToken)
    {
        var track = await SeedTrackAsync(
            name: "Databases",
            description:
                "Relational databases and SQL — from DBMS concepts and schema design through normalization, " +
                "querying, data manipulation, and data-definition.",
            iconCode: "DB",
            position: 1,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 0,
            name: "Database Fundamentals",
            description:
                "What a DBMS is, keys (primary, natural, foreign), unique constraints, and relationships " +
                "(one-to-many, many-to-many) — the building blocks of a relational schema.",
            iconCode: "DB",
            buildQuestions: SqlQuestions.DatabaseFundamentals,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 1,
            name: "Normalization",
            description:
                "Database normalization — first, second, and third normal forms, and when normalizing helps " +
                "versus when a denormalized shape fits the workload.",
            iconCode: "NF",
            buildQuestions: SqlQuestions.Normalization,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 2,
            name: "Querying",
            description:
                "The SELECT statement — joins, GROUP BY and HAVING, aggregates, DISTINCT, set operations " +
                "(UNION/INTERSECT), and common table expressions.",
            iconCode: "SEL",
            buildQuestions: SqlQuestions.Querying,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 3,
            name: "Data Manipulation",
            description:
                "Changing data — INSERT, UPDATE, and DELETE — plus transactions and the ACID properties " +
                "that keep those changes consistent.",
            iconCode: "DML",
            buildQuestions: SqlQuestions.DataManipulation,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 4,
            name: "Schema Definition",
            description:
                "Data-definition language — CREATE and ALTER, views, TRUNCATE, and indexes: the statements " +
                "that shape and tune the database structure itself.",
            iconCode: "DDL",
            buildQuestions: SqlQuestions.SchemaDefinition,
            cancellationToken);
    }

    private async Task SeedFrontEndTrackAsync(CancellationToken cancellationToken)
    {
        var track = await SeedTrackAsync(
            name: "Front-End",
            description:
                "Front-end web fundamentals — the JavaScript language, its asynchronous model, TypeScript's " +
                "type system, and core HTML and CSS.",
            iconCode: "FE",
            position: 2,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 0,
            name: "JavaScript",
            description:
                "Core JavaScript — strict equality and type coercion, scope and hoisting, closures, the this " +
                "binding, null vs undefined, destructuring, spread/rest, and array methods.",
            iconCode: "JS",
            buildQuestions: FrontEndQuestions.JavaScript,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 1,
            name: "Async & Events",
            description:
                "JavaScript's concurrency model — the event loop, promises and async/await, Promise.all, and " +
                "DOM event bubbling.",
            iconCode: "ASY",
            buildQuestions: FrontEndQuestions.AsyncAndEvents,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 2,
            name: "TypeScript",
            description:
                "TypeScript's type system — how it compiles to JavaScript, generics, and type narrowing.",
            iconCode: "TS",
            buildQuestions: FrontEndQuestions.TypeScript,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 3,
            name: "HTML & CSS",
            description:
                "Core HTML and CSS — semantic markup, the box model, specificity, positioning, flexbox and " +
                "grid, units, media queries, and pseudo-classes vs pseudo-elements.",
            iconCode: "CSS",
            buildQuestions: FrontEndQuestions.HtmlAndCss,
            cancellationToken);
    }

    private async Task SeedEngineeringTrackAsync(CancellationToken cancellationToken)
    {
        var track = await SeedTrackAsync(
            name: "Engineering Practices",
            description:
                "Software engineering workflow and craftsmanship — Git and version control, continuous " +
                "integration and delivery, and clean-code practices.",
            iconCode: "ENG",
            position: 3,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 0,
            name: "Git & Version Control",
            description:
                "Git and version control — the staging area, branches, merge vs rebase, fast-forward, " +
                "cherry-pick, stash, reset vs revert, conventional commits, and pull requests.",
            iconCode: "GIT",
            buildQuestions: EngineeringPracticesQuestions.GitAndVersionControl,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 1,
            name: "CI/CD",
            description:
                "Continuous integration and delivery/deployment — pipelines, build-once-deploy-many, " +
                "feature-branch and trunk-based flow, blue-green deployment, and semantic versioning.",
            iconCode: "CI",
            buildQuestions: EngineeringPracticesQuestions.ContinuousIntegrationDelivery,
            cancellationToken);

        await SeedCategoryIfMissingAsync(track, position: 2,
            name: "Clean Code",
            description:
                "Clean-code craftsmanship — meaningful names, DRY and YAGNI, code smells, refactoring, " +
                "technical debt, the Boy Scout rule, code review, static analysis, and the test pyramid.",
            iconCode: "CLN",
            buildQuestions: EngineeringPracticesQuestions.CleanCode,
            cancellationToken);
    }

    private async Task<Track> SeedTrackAsync(
        string name,
        string description,
        string iconCode,
        int position,
        CancellationToken cancellationToken)
    {
        var existing = await db.Tracks.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        if (existing is not null)
        {
            logger.LogInformation("Seed skipped — track {TrackName} already exists", name);
            return existing;
        }

        var track = new Track(Guid.NewGuid(), name, description, iconCode, position);
        db.Tracks.Add(track);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded track {TrackName}", name);
        return track;
    }

    private async Task SeedCategoryIfMissingAsync(
        Track track,
        int position,
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

        var category = new Category(Guid.NewGuid(), track.Id, name, description, iconCode, position);
        var questions = buildQuestions(category.Id);
        var quiz = Quiz.Create(Guid.NewGuid(), category.Id, questions);

        db.Categories.Add(category);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded category {CategoryName} with {QuestionCount} questions under track {TrackName}, quiz {QuizId}",
            name, questions.Count, track.Name, quiz.Id);
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

    /// <summary>
    /// Gives the shared demo account a realistic, always-fresh quiz history so the dashboard, history,
    /// category progress and result screens have content to show. Unlike the rest of the seeder this is
    /// deliberately <em>not</em> idempotent: it wipes the demo user's prior attempts and regenerates them
    /// with dates relative to <c>now</c> on every run, so a returning visitor never lands on a stale
    /// dashboard (streak decayed to 0, empty weekly bars). The wipe also self-cleans whatever a previous
    /// visitor did on the demo account. Only the demo user is touched — real accounts are never wiped.
    /// </summary>
    private async Task SeedDemoHistoryAsync(CancellationToken cancellationToken)
    {
        var demoUser = await userManager.FindByEmailAsync(DemoUserEmail);
        if (demoUser is null)
        {
            return;
        }

        var userId = demoUser.Id;

        // Owned answers load with their attempts and cascade-delete with them.
        var existing = await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
        if (existing.Count > 0)
        {
            db.QuizAttempts.RemoveRange(existing);
            await db.SaveChangesAsync(cancellationToken);
        }

        var quizzes = await db.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
            .ToListAsync(cancellationToken);

        // Spread the history across up to eight distinct categories so category-strength and recent
        // activity read varied. Stable ordering keeps the selection deterministic across runs.
        var selected = quizzes
            .Where(q => q.Questions.Count > 0)
            .OrderBy(q => q.CategoryId)
            .Take(8)
            .ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var attempts = new List<QuizAttempt>();
        var plan = BuildDemoHistoryPlan();

        for (var i = 0; i < plan.Count; i++)
        {
            var (dayOffset, targetScore) = plan[i];
            var quiz = selected[i % selected.Count];
            var questions = quiz.Questions.ToList();
            var total = questions.Count;

            var day = now.AddDays(-dayOffset);
            // Late-evening study session; stagger start times so same-day attempts don't collide.
            var startedAt = new DateTimeOffset(day.Year, day.Month, day.Day, 19, 0, 0, TimeSpan.Zero)
                .AddMinutes(i * 7 % 45);
            var attempt = QuizAttempt.Start(Guid.NewGuid(), userId, quiz.Id, startedAt);

            var correctTarget = Math.Clamp(
                (int)Math.Round(targetScore / 100.0 * total, MidpointRounding.AwayFromZero), 0, total);
            var correctCount = 0;
            for (var qi = 0; qi < total; qi++)
            {
                var question = questions[qi];
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                var pickCorrect = qi < correctTarget && correctOption is not null;
                var chosen = pickCorrect
                    ? correctOption
                    : question.Options.FirstOrDefault(o => !o.IsCorrect) ?? correctOption;
                if (pickCorrect)
                {
                    correctCount++;
                }

                attempt.SubmitAnswer(question.Id, chosen?.Id, startedAt.AddSeconds(15 + qi * 8));
            }

            var percentage = total == 0 ? 0 : Math.Round(correctCount * 100.0 / total, 1);
            attempt.Complete(startedAt.AddMinutes(4).AddSeconds(20), percentage);
            attempts.Add(attempt);
        }

        db.QuizAttempts.AddRange(attempts);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Refreshed demo history — {Count} attempts across {Categories} categories for {Email}",
            attempts.Count, selected.Count, DemoUserEmail);
    }

    /// <summary>
    /// Deterministic (day-offset, target-score%) schedule for the demo history: twelve consecutive
    /// days ending today build a ~12-day streak, scores trend gently upward for a positive dashboard
    /// trend, and a handful of extra/older sessions add texture and fill the 14-day activity sparkline.
    /// </summary>
    private static IReadOnlyList<(int DayOffset, double TargetScore)> BuildDemoHistoryPlan()
    {
        var plan = new List<(int, double)>();

        // Twelve-day streak (offsets 11..0), scores rising from ~62% to ~90%.
        for (var dayOffset = 11; dayOffset >= 0; dayOffset--)
        {
            var target = Math.Min(94, 62 + (11 - dayOffset) * 2.5);
            plan.Add((dayOffset, target));
        }

        // Extra same-day sessions and a couple of older ones (still inside the 14-day window).
        plan.Add((8, 71));
        plan.Add((6, 79));
        plan.Add((4, 85));
        plan.Add((2, 90));
        plan.Add((1, 92));
        plan.Add((13, 57));
        plan.Add((9, 66));

        return plan;
    }
}
