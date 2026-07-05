using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the Databases track — relational database and SQL fundamentals covering
/// DBMS concepts, keys, constraints, relationships, normalization, the SELECT statement, DML/TCL,
/// and DDL. The per-question factory methods are partitioned into subcategory lists
/// (DatabaseFundamentals, Normalization, Querying, DataManipulation, SchemaDefinition).
/// </summary>
/// <remarks>
/// All questions are single-correct (often NOT-questions or "which is true/false") to satisfy the
/// <c>MultipleChoice</c> Domain invariant (exactly one correct option per question).
/// </remarks>
public static class SqlQuestions
{
    public static IReadOnlyList<Question> DatabaseFundamentals(Guid categoryId) =>
    [
        Q01_WhatIsDbms(categoryId),
        Q02_PrimaryKeyIncorrect(categoryId),
        Q03_NaturalKey(categoryId),
        Q04_UniqueConstraintNulls(categoryId),
        Q05_OneToMany(categoryId),
        Q06_ManyToMany(categoryId),
        Q07_SqlDeclarative(categoryId),
        Q08_SqlDisadvantage(categoryId),
        Q21_ForeignKey(categoryId),
    ];

    public static IReadOnlyList<Question> Normalization(Guid categoryId) =>
    [
        Q12_FirstNormalForm(categoryId),
        Q13_SecondNormalForm(categoryId),
        Q14_NormalizationWorkloads(categoryId),
        Q28_ThirdNormalForm(categoryId),
    ];

    public static IReadOnlyList<Question> Querying(Guid categoryId) =>
    [
        Q09_HavingWithNull(categoryId),
        Q10_CteKeyword(categoryId),
        Q11_IntersectFalse(categoryId),
        Q22_InnerVsLeftJoin(categoryId),
        Q23_GroupBy(categoryId),
        Q24_WhereVsHaving(categoryId),
        Q25_AggregateCount(categoryId),
        Q26_Distinct(categoryId),
        Q29_UnionVsUnionAll(categoryId),
    ];

    public static IReadOnlyList<Question> DataManipulation(Guid categoryId) =>
    [
        Q15_InsertNull(categoryId),
        Q16_UpdateAllRows(categoryId),
        Q17_DeleteSyntax(categoryId),
        Q30_AcidProperties(categoryId),
    ];

    public static IReadOnlyList<Question> SchemaDefinition(Guid categoryId) =>
    [
        Q18_Truncate(categoryId),
        Q19_AlterTable(categoryId),
        Q20_View(categoryId),
        Q27_IndexPurpose(categoryId),
    ];

    private static Question Q01_WhatIsDbms(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which of the following best describes a DBMS?",
            explanation:
                "A Database Management System is software that lets users define, store, query, and " +
                "manipulate data while controlling access, concurrency, and recovery. It is far more " +
                "than a single table or a backup tool.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Software that lets users define, store, manipulate, and control access to data", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A single table that stores all of an application's data",                        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A query language used only for reading data",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A tool whose only job is to restore data from backups",                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_PrimaryKeyIncorrect(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which statement about a primary key is INCORRECT?",
            explanation:
                "A primary key must be unique and non-null, and a table has exactly one. The false " +
                "statement is that its values may contain duplicates — uniqueness is precisely what a " +
                "primary key guarantees.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Primary key values can contain duplicates", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A table can have only one primary key",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The primary key enforces uniqueness",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The primary key cannot contain NULL values", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_NaturalKey(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which statement best describes a natural key?",
            explanation:
                "A natural key is built from attributes that already carry real-world meaning (e.g. an " +
                "SSN or product code). A key generated by the database with no inherent meaning is a " +
                "surrogate key.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It is based on attributes that have real-world meaning, such as an SSN or product code", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It is generated by the database and has no real-world meaning",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It is always an auto-incrementing integer",                                              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It can never be used as a primary key",                                                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_UniqueConstraintNulls(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which statement about the UNIQUE constraint is correct?",
            explanation:
                "UNIQUE forbids duplicate non-null values but does not prevent NULLs — enforcing " +
                "non-null is the job of the NOT NULL constraint. UNIQUE can be declared at both the " +
                "column and table level, and it is not a synonym for PRIMARY KEY.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It does not prevent NULL values — preventing NULLs is the job of NOT NULL", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It guarantees that a column can never contain NULL",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It can only be defined at the table level, never the column level",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is just another name for PRIMARY KEY",                                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_OneToMany(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which pair of entities is a classic one-to-many (1:N) relationship?",
            explanation:
                "One customer can place many orders (1:N). The other pairs are one-to-one: a laptop has " +
                "a single serial number, a user has one driving license, and a username maps to one password.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Customer and order",        isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Laptop and serial number",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "User and driving license",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Username and password",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_ManyToMany(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "A \"student and course\" relationship is many-to-many. " +
                "How is it represented in a relational schema?",
            explanation:
                "Relational tables cannot model M:N directly. A junction (join) table holds foreign keys " +
                "to both sides, turning one M:N relationship into two 1:N relationships.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "With a junction (join) table holding foreign keys to both tables", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "With a single foreign key column on the student table",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "With a CHECK constraint on each table",                            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "No extra structure is needed — relational tables model M:N directly", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_SqlDeclarative(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "SQL is described as a \"declarative\" language. What does that mean?",
            explanation:
                "In a declarative language you state WHAT result you want and let the query optimizer " +
                "decide HOW to produce it. You don't write the loops or the access path yourself.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "You specify what result you want, not how to compute it",          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "You must specify the exact algorithm, including loops",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It can only be executed inside stored procedures",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It compiles directly to machine code before running",             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_SqlDisadvantage(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which of the following is a DISADVANTAGE of SQL rather than an advantage?",
            explanation:
                "Being standardized, declarative, and largely DBMS-independent are advantages. The " +
                "drawback is that core SQL has no loops, conditionals, or variables — it relies on " +
                "procedural extensions such as PL/pgSQL or T-SQL for those.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It needs procedural extensions (PL/pgSQL, T-SQL) for loops and conditionals", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It is standardized across vendors",                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It is DBMS-independent for basic queries",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is declarative",                                                          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_HavingWithNull(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "Table `A` has an `Amount` column with values 10, NULL, 30, 10, NULL, 50. " +
                "What does this query return?\n\n" +
                "```sql\n" +
                "SELECT COUNT(*) AS cnt, Amount\n" +
                "FROM A\n" +
                "GROUP BY Amount\n" +
                "HAVING Amount < 40;\n" +
                "```",
            explanation:
                "GROUP BY produces groups 10 (count 2), NULL (count 2), 30 (count 1), 50 (count 1). " +
                "HAVING keeps only groups where the predicate is TRUE: `NULL < 40` evaluates to NULL " +
                "(not TRUE), so the NULL group is dropped, and 50 fails `< 40`. Result: (2, 10) and (1, 30).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Two groups: count 2 for Amount 10, and count 1 for Amount 30", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Three groups, including a row for the NULL group",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "One group: count 1 for Amount 50",                            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Six rows, one for each original row",                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_CteKeyword(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which keyword introduces a Common Table Expression (CTE)?",
            explanation:
                "A CTE is defined with `WITH cte_name AS (SELECT ...)`, then referenced like a table in " +
                "the main query. CTEs improve readability and let you reuse a subquery without duplicating it.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "WITH",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "WHERE",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "HAVING", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "CTE",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_IntersectFalse(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which statement about the INTERSECT set operator is FALSE?",
            explanation:
                "INTERSECT returns only the rows common to both result sets — a smaller set, not a " +
                "combined one. \"Combining all rows into one larger set\" describes UNION. Both queries " +
                "must have the same number of columns with compatible data types.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "INTERSECT combines all rows from both queries into one larger result set", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It returns only rows that appear in both result sets",                     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Both SELECT statements must contain the same number of columns",           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Corresponding columns must have compatible data types",                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_FirstNormalForm(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which requirement is a rule of First Normal Form (1NF)?",
            explanation:
                "1NF requires atomic (single-valued) cells, among rules like unique column names and " +
                "unique rows. Full dependency on the PK is a 2NF rule; eliminating transitive " +
                "dependencies is a 3NF rule.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Every cell must contain a single (atomic) value",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Every non-key column is fully dependent on the primary key", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "There is no transitive dependency for non-key attributes",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Every foreign key must reference a primary key",             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_SecondNormalForm(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which statement correctly defines Second Normal Form (2NF)?",
            explanation:
                "2NF has two parts: the relation is already in 1NF, AND every non-key attribute is fully " +
                "functionally dependent on the whole primary key (no partial dependencies). Transitive " +
                "dependency relates to 3NF, not 2NF.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The relation is in 1NF and every non-key attribute is fully functionally dependent on the primary key", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Every non-key attribute is transitively dependent on the primary key",                                  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The relation is in 1NF and non-key attributes are transitively dependent on the primary key",           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is any relation that has no primary key",                                                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_NormalizationWorkloads(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which statement about normalization and database workloads is correct?",
            explanation:
                "Analytical (OLAP) systems favor denormalization (star/snowflake schemas) to speed up " +
                "large reads. Higher normalization produces MORE tables, not fewer, and OLTP systems are " +
                "built precisely around frequent INSERT/UPDATE/DELETE.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "OLAP/analytical databases often use denormalization to speed up large read queries", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A higher level of normalization always means fewer tables",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "OLTP databases rarely perform INSERT or UPDATE operations",                          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "OLAP databases require the highest possible level of normalization",                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_InsertNull(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which INSERT statement correctly stores a NULL value in `column2`?",
            explanation:
                "The bare keyword `NULL` (no quotes) inserts an actual NULL. `\"\"` and `\"NULL\"` use " +
                "double quotes, which denote an identifier in standard SQL, not a value; an empty string " +
                "`''` is also not NULL. Omitting the value entirely supplies too few values.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "INSERT INTO t (column1, column2) VALUES (value1, NULL)",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "INSERT INTO t (column1, column2) VALUES (value1, \"\")",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "INSERT INTO t (column1, column2) VALUES (value1, \"NULL\")", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "INSERT INTO t (column1, column2) VALUES (value1)",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_UpdateAllRows(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which UPDATE statement modifies `column1` for ALL rows in a table?",
            explanation:
                "An UPDATE with no WHERE clause applies to every row. Adding `WHERE` filters the rows; the " +
                "parenthesized and `VALUES` forms are invalid UPDATE syntax (VALUES belongs to INSERT).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "UPDATE t SET column1 = value1",                   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "UPDATE t SET column1 = value1 WHERE condition",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "UPDATE t (column1) SET (value1)",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "UPDATE t (column1) VALUES (value1)",              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_DeleteSyntax(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which is the correct syntax for a DELETE statement?",
            explanation:
                "Standard SQL is `DELETE FROM table_name WHERE condition`. The `FROM` keyword is required, " +
                "and the condition must follow `WHERE`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "DELETE FROM table_name WHERE condition", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DELETE table_name WHERE condition",      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "DELETE condition FROM table_name",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "DELETE (condition) table_name",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_Truncate(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the TRUNCATE statement do?",
            explanation:
                "TRUNCATE removes all rows but keeps the table's structure (columns, constraints, indexes). " +
                "Dropping the structure too is DROP TABLE; deleting selected rows with a WHERE clause is DELETE.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Removes all rows from a table but preserves its structure",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Removes the data and permanently drops the table structure",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Deletes only the rows that match a WHERE clause",                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Rolls back the most recent transaction",                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_AlterTable(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which DDL statement modifies the structure of an existing table?",
            explanation:
                "ALTER TABLE changes an existing table's structure (add/drop columns, add constraints). " +
                "CREATE TABLE makes a new one, DROP TABLE removes it, and INSERT INTO is DML, not DDL.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "ALTER TABLE",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "CREATE TABLE", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "DROP TABLE",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "INSERT INTO",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_View(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is a view in PostgreSQL?",
            explanation:
                "A view is a named, stored SELECT statement that behaves like a virtual table. It stores " +
                "no data of its own — it re-runs its query and fetches data at access time.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A virtual table defined by a stored SELECT statement; it does not store data itself", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A physical copy of a table stored on disk",                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A backup snapshot of a table's data",                                                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "An index that speeds up queries",                                                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q21_ForeignKey(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does a FOREIGN KEY constraint enforce?",
            explanation:
                "A foreign key enforces referential integrity: each value in the referencing column must match " +
                "an existing value in the referenced table's primary/unique key (or be NULL). It prevents " +
                "'orphan' rows that point to records which don't exist.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "That a column's values must reference existing rows in another table", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "That every value in the column must be unique",                        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "That the column can never contain NULL",                               isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "That the table can have only one such column",                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q22_InnerVsLeftJoin(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How does a LEFT JOIN differ from an INNER JOIN?",
            explanation:
                "An INNER JOIN returns only rows that have a match in both tables. A LEFT JOIN returns all rows " +
                "from the left table, plus matching rows from the right; where there is no match, the right " +
                "side's columns are NULL. LEFT JOIN is how you keep unmatched left-table rows.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "LEFT JOIN keeps all left-table rows (NULLs where no right match); INNER JOIN keeps only matched rows", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "LEFT JOIN keeps only matched rows; INNER JOIN keeps all rows from both tables",                         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They produce identical results but LEFT JOIN is faster",                                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "LEFT JOIN removes duplicate rows; INNER JOIN keeps them",                                                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q23_GroupBy(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the GROUP BY clause do in a SELECT statement?",
            explanation:
                "GROUP BY collapses rows that share the same values in the grouping columns into a single " +
                "summary row, so aggregate functions (COUNT, SUM, AVG, …) can be computed per group. Columns " +
                "in the SELECT list must either appear in GROUP BY or be inside an aggregate.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Groups rows with equal values so aggregates can be computed per group", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Sorts the result set by the named columns",                             isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Removes duplicate rows from the result",                                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Filters rows before they are selected",                                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q24_WhereVsHaving(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the difference between WHERE and HAVING?",
            explanation:
                "WHERE filters individual rows before grouping and cannot reference aggregate functions. HAVING " +
                "filters groups after GROUP BY and aggregation, so it can use aggregates like COUNT(*) > 5. In " +
                "short: WHERE is pre-aggregation, HAVING is post-aggregation.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "WHERE filters rows before grouping; HAVING filters groups after aggregation", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "WHERE filters after grouping; HAVING filters before grouping",               isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are interchangeable in every query",                                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "WHERE works only with JOINs; HAVING works only with subqueries",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q25_AggregateCount(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How does `COUNT(column)` differ from `COUNT(*)`?",
            explanation:
                "`COUNT(*)` counts all rows, including those with NULLs. `COUNT(column)` counts only rows where " +
                "that column is non-NULL. So if a column has NULLs, `COUNT(column)` returns fewer than " +
                "`COUNT(*)`. `COUNT(DISTINCT column)` counts distinct non-NULL values.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`COUNT(*)` counts all rows; `COUNT(column)` counts only rows where that column is non-NULL", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They always return the same number",                                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`COUNT(column)` counts all rows; `COUNT(*)` ignores NULLs",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`COUNT(*)` counts distinct values only",                                                      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q26_Distinct(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the DISTINCT keyword do in `SELECT DISTINCT ...`?",
            explanation:
                "DISTINCT removes duplicate rows from the result set, returning only unique combinations of the " +
                "selected columns. It operates on the entire selected row, not a single column, so " +
                "`SELECT DISTINCT a, b` returns unique (a, b) pairs.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Eliminates duplicate rows, returning unique combinations of the selected columns", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Sorts the result set in ascending order",                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Counts the number of rows returned",                                               isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Selects only the first row of each group",                                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q27_IndexPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the main purpose of a database index, and what is its main trade-off?",
            explanation:
                "An index speeds up read queries (lookups, joins, sorting on the indexed columns) by letting " +
                "the engine avoid full table scans. The trade-off is that indexes consume extra storage and " +
                "slow down writes (INSERT/UPDATE/DELETE), since the index must be maintained on every change.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It speeds up reads on indexed columns, at the cost of extra storage and slower writes", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It speeds up writes, at the cost of slower reads",                                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It guarantees uniqueness of every column in the table",                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It compresses the table to save disk space with no downside",                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q28_ThirdNormalForm(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "A table is in 2NF. What additional condition must it meet to be in Third Normal Form (3NF)?",
            explanation:
                "3NF requires that no non-key column depends on another non-key column — i.e. there are no " +
                "transitive dependencies on the primary key. Every non-key attribute must depend on the key, " +
                "the whole key, and nothing but the key. Moving the transitively dependent columns into their " +
                "own table restores 3NF.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "No non-key column may depend on another non-key column (no transitive dependencies)", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Every column must contain only a single atomic value",                                isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The table must have no foreign keys",                                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Every column must be indexed",                                                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q29_UnionVsUnionAll(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the difference between UNION and UNION ALL?",
            explanation:
                "Both combine the result sets of two queries with compatible columns. UNION removes duplicate " +
                "rows (which requires an extra sort/dedup step), while UNION ALL returns every row including " +
                "duplicates and is therefore faster. Use UNION ALL when you know there are no duplicates or " +
                "want to keep them.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "UNION removes duplicate rows; UNION ALL keeps all rows including duplicates", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "UNION ALL removes duplicates; UNION keeps them",                             isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "UNION joins tables horizontally; UNION ALL joins them vertically",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They are identical in every respect",                                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q30_AcidProperties(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "In the ACID properties of a transaction, what does 'Atomicity' guarantee?",
            explanation:
                "Atomicity guarantees that a transaction is all-or-nothing: either every statement in it " +
                "commits, or — if any fails — the whole transaction rolls back, leaving the database as if it " +
                "never ran. (The others: Consistency, Isolation, Durability.)",
            options:
            [
                new Option(Guid.NewGuid(), qid, "All statements in the transaction succeed together, or none take effect", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Committed data survives a subsequent system crash",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Concurrent transactions do not interfere with each other",                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The database moves from one valid state to another",                      isCorrect: false, orderIndex: 3),
            ]);
    }
}
