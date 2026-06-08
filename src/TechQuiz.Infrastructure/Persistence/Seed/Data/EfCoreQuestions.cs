using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "EF Core" category. Content adapted from the EPAM .NET
/// Fundamentals course, module 013 (Entity Framework Core) graded and ungraded quizzes —
/// covering the ORM role, DbContext/DbSet, migrations, model configuration, loading
/// strategies, change tracking, and persistence behaviour.
/// </summary>
/// <remarks>
/// Several source questions were "select ALL correct" / "pick TWO" items. They were
/// rephrased to single-correct (NOT-questions or "which is true") to satisfy the
/// <c>MultipleChoice</c> Domain invariant (exactly one correct option per question).
/// </remarks>
public static class EfCoreQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_WhatIsEfCore(categoryId),
        Q02_AdoNetAbstraction(categoryId),
        Q03_DbContextNotResponsibility(categoryId),
        Q04_SaveChanges(categoryId),
        Q05_MigrationsPurpose(categoryId),
        Q06_AddMigrationCommand(categoryId),
        Q07_ModelApproaches(categoryId),
        Q08_LoadingPatternNotUsed(categoryId),
        Q09_DbSet(categoryId),
        Q10_AddVsAttach(categoryId),
        Q11_IncludeEagerLoad(categoryId),
        Q12_TableAttribute(categoryId),
        Q13_FluentApiHasKey(categoryId),
        Q14_RemoveSaveChanges(categoryId),
        Q15_ConfigurationPrecedence(categoryId),
        Q16_LazyLoading(categoryId),
        Q17_ChangeTracker(categoryId),
        Q18_AsNoTracking(categoryId),
        Q19_OptimisticConcurrency(categoryId),
        Q20_DatabaseUpdateCommand(categoryId),
    ];

    private static Question Q01_WhatIsEfCore(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which statement about EF Core is true?",
            explanation:
                "EF Core is an Object-Relational Mapper (ORM): it maps database tables and rows to .NET " +
                "entity classes, removing most hand-written mapping and SQL for typical CRUD. It supports " +
                "many providers, not just SQL Server.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It is an ORM that maps database tables to entity classes", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It only works with SQL Server",                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It requires you to write all mapping code manually",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is a replacement for the C# language",                   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_AdoNetAbstraction(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What level of abstraction does ADO.NET provide compared to EF Core?",
            explanation:
                "ADO.NET is a low-level data access technology: you manage connections, commands, and SQL " +
                "by hand. EF Core sits on top of it as a high-level ORM that generates SQL and maps results " +
                "to entities for you.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A low-level technology that requires SQL and connections to be handled manually", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A high-level ORM that maps entities automatically",                              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A tool that generates entity classes with no code",                              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A universal database migration tool",                                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_DbContextNotResponsibility(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which of the following is NOT a responsibility of the EF Core `DbContext`?",
            explanation:
                "DbContext manages database connections, maps CLR entities to the schema, and runs queries " +
                "and persistence (SaveChanges). User authentication is an application/identity concern, not " +
                "something DbContext provides.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Authenticating application users",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Managing the database connection",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Mapping entities to the database schema",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Querying and persisting data",                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_SaveChanges(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which `DbContext` method persists tracked changes to the database?",
            explanation:
                "`SaveChanges` (or `SaveChangesAsync`) writes all tracked inserts, updates, and deletes in a " +
                "single transaction. `CommitChanges` is not an EF API, and updates happen on tracked entities " +
                "followed by SaveChanges — not via a `DbContext.Update` call alone.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "SaveChanges / SaveChangesAsync", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "CommitChanges",                  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Dispose",                        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Flush",                          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_MigrationsPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the purpose of migrations in EF Core?",
            explanation:
                "Migrations evolve the database schema incrementally to keep it in sync with the EF model " +
                "as it changes over time. They are not a backup mechanism or an authentication feature.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Managing incremental changes to the database schema", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Backing up and restoring database data",              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Authenticating users against the database",            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Caching query results for performance",                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_AddMigrationCommand(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which CLI command creates a new migration in EF Core?",
            explanation:
                "`dotnet ef migrations add <Name>` scaffolds a new migration file from model changes. " +
                "`database update` applies migrations (it does not add one), and `dbcontext scaffold` is " +
                "reverse-engineering from an existing database.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "dotnet ef migrations add <Name>", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "dotnet ef database update",       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "dotnet ef migrations generate",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "dotnet ef dbcontext scaffold",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_ModelApproaches(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which model-development approach is NOT part of the EF Core mainstream workflow?",
            explanation:
                "EF Core supports code-first (C# classes + migrations) and database-first (scaffold from an " +
                "existing database). Model-first (the EDMX/designer workflow from EF6) is not part of EF Core.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Model-first (EDMX/designer workflow)",                 isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Code-first (classes + migrations)",                    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Database-first (scaffold from an existing database)",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Code-first with EnsureCreated",                        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_LoadingPatternNotUsed(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which related-data loading pattern is NOT a named strategy in EF Core?",
            explanation:
                "EF Core documents three loading strategies: eager (`Include`), explicit (`Load`), and lazy " +
                "(proxies, loaded on access). \"Implicit loading\" is not a named EF Core strategy.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Implicit loading", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Eager loading",    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Explicit loading", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Lazy loading",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_DbSet(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which type represents a collection of all entities of a given type and lets you query them?",
            explanation:
                "`DbSet<TEntity>` is exposed on the context (e.g. `DbSet<Blog> Blogs`) and is the queryable " +
                "entry point for one entity type. `DbContext` represents the whole session, not a single set.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "DbSet<TEntity>",     isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DbContext",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "EntityEntry<T>",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "IQueryable",         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_AddVsAttach(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "To insert a NEW entity, which method marks it with the `Added` state so `SaveChanges` issues an INSERT?",
            explanation:
                "`Add` marks an entity as `Added`, producing an INSERT on `SaveChanges`. `Attach` begins " +
                "tracking with the `Unchanged` state (an existing row), so it does not insert. `Entry` just " +
                "returns tracking info, and `Update` marks all properties modified for an UPDATE.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Add",    isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Attach", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Entry",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Update", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_IncludeEagerLoad(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "Which expression eagerly loads each company together with its related users?\n\n" +
                "```csharp\n" +
                "var companies = context.Companies.____;\n" +
                "```",
            explanation:
                "`Include(c => c.Users)` is eager loading — it pulls the related users in the same query. " +
                "`WithUsers()` is not an EF API, and a projection or a bare `Load()` does not express this " +
                "the same way.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Include(c => c.Users)",                         isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "WithUsers()",                                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Select(c => new { c })",                        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Load(c => c.Users)",                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_TableAttribute(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "Which data annotation maps the `Author` entity to a table named `CustomAuthors`?\n\n" +
                "```csharp\n" +
                "[____(\"CustomAuthors\")]\n" +
                "public class Author { }\n" +
                "```",
            explanation:
                "`[Table(\"CustomAuthors\")]` sets the table name. `[Column]` renames a column, `[Schema]` is " +
                "not a standalone annotation, and `[InverseProperty]` pairs navigations.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[Table]",           isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[Column]",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Entity]",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[InverseProperty]", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_FluentApiHasKey(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "In `OnModelCreating`, which Fluent API call sets `TIN` as the primary key of `Company`?",
            explanation:
                "`HasKey(c => c.TIN)` configures the primary key. `HasIndex` creates an index (not a PK), and " +
                "`IsRequired` only enforces non-null.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "modelBuilder.Entity<Company>().HasKey(c => c.TIN)",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "modelBuilder.Entity<Company>().HasIndex(c => c.TIN)",    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "modelBuilder.Entity<Company>().IsRequired(c => c.TIN)",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "modelBuilder.Entity<Company>().HasKey(c => c.Name)",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_RemoveSaveChanges(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "What SQL does `SaveChanges` run for the persistence step here?\n\n" +
                "```csharp\n" +
                "var order = context.Orders.FirstOrDefault();\n" +
                "context.Orders.Remove(order);\n" +
                "context.SaveChanges();\n" +
                "```",
            explanation:
                "After `Remove`, the entity is in the `Deleted` state, so `SaveChanges` issues a " +
                "`DELETE FROM Orders WHERE Id = <id>` for that single row. The `FirstOrDefault` earlier was a " +
                "separate SELECT.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "DELETE FROM Orders WHERE Id = <id of the loaded order>", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DELETE FROM Orders (all rows)",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "UPDATE Orders SET Id = NULL",                            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Only a SELECT — nothing is deleted",                     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_ConfigurationPrecedence(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "When conventions, data annotations, and the Fluent API conflict, which configuration wins?",
            explanation:
                "Precedence is Fluent API > data annotations > conventions. The Fluent API (in " +
                "`OnModelCreating`) has the final say and overrides both annotations and default conventions.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The Fluent API",     isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Data annotations",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Default conventions", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Whichever is declared last in the file", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_LazyLoading(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which statement best describes lazy loading in EF Core?",
            explanation:
                "Lazy loading defers fetching related data until the navigation property is first accessed, " +
                "issuing a separate query at that moment. It requires proxies (or manual implementation) and " +
                "can cause the N+1 query problem if used carelessly.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Related data is fetched only when its navigation property is first accessed", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "All related data is always loaded with the parent query",                    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Related data is never loaded automatically and must be projected manually",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It loads data eagerly using the Include method",                             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_ChangeTracker(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does EF Core's change tracker do?",
            explanation:
                "The change tracker records the state of tracked entities (Added, Modified, Deleted, " +
                "Unchanged) so that `SaveChanges` knows which INSERT/UPDATE/DELETE statements to generate.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Tracks the state of entities so SaveChanges can generate the right SQL", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Encrypts entity data before saving",                                    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Logs every SQL query to the console",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Validates user permissions on each entity",                             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_AsNoTracking(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Why would you add `AsNoTracking()` to a query?",
            explanation:
                "`AsNoTracking()` tells EF Core not to track the returned entities, which is faster and uses " +
                "less memory for read-only queries. The trade-off: those entities cannot be updated via " +
                "`SaveChanges` because no change tracking exists for them.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To improve performance for read-only queries by skipping change tracking", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To make the returned entities update faster on SaveChanges",              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To force eager loading of all navigation properties",                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To wrap the query in an explicit transaction",                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_OptimisticConcurrency(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "How does EF Core implement optimistic concurrency control?",
            explanation:
                "EF Core uses a concurrency token (e.g. a `[Timestamp]`/rowversion column). On update it adds " +
                "the original token value to the WHERE clause; if another user changed the row, zero rows " +
                "match and EF throws a `DbUpdateConcurrencyException`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "With a concurrency token checked in the UPDATE's WHERE clause; a mismatch throws DbUpdateConcurrencyException", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "By locking the row for the entire lifetime of the DbContext",                                                  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "By disabling all concurrent access to the database",                                                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "By automatically retrying the save until it succeeds",                                                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_DatabaseUpdateCommand(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which CLI command applies pending migrations to the database?",
            explanation:
                "`dotnet ef database update` applies any pending migrations to bring the database schema up " +
                "to date. `migrations add` only creates a new migration file; it does not touch the database.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "dotnet ef database update",       isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "dotnet ef migrations add",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "dotnet ef database migrate",      isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "dotnet ef migrations apply",      isCorrect: false, orderIndex: 3),
            ]);
    }
}
