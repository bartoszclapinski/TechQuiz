using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "ADO.NET" category. Content adapted from the EPAM .NET
/// Fundamentals course, module 012 (ADO.NET) graded and ungraded quizzes, expanded with
/// standard interview ground — the abstract provider model (<c>DbConnection</c>,
/// <c>DbCommand</c>, <c>DbDataReader</c>, <c>DbParameter</c>), command execution methods,
/// parameterized queries, the connected vs disconnected models, transactions, and
/// connection pooling.
/// </summary>
/// <remarks>
/// Source quiz items are single-answer; a few expanded questions follow the same shape to
/// satisfy the <c>MultipleChoice</c> Domain invariant (exactly one correct option per question).
/// </remarks>
public static class AdoNetQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_OpenConnection(categoryId),
        Q02_ExecuteScalar(categoryId),
        Q03_ExecuteNonQuery(categoryId),
        Q04_ExecuteReader(categoryId),
        Q05_DataReaderCursor(categoryId),
        Q06_DbConnectionRole(categoryId),
        Q07_CreateParameter(categoryId),
        Q08_SqlInjection(categoryId),
        Q09_ReaderRead(categoryId),
        Q10_DataSetDisconnected(categoryId),
        Q11_DataAdapterFill(categoryId),
        Q12_UsingDisposes(categoryId),
        Q13_ConnectionPooling(categoryId),
        Q14_StoredProcedure(categoryId),
        Q15_DbProviderFactory(categoryId),
        Q16_Transaction(categoryId),
        Q17_SqlCommandProvider(categoryId),
        Q18_ExecuteScalarReturn(categoryId),
        Q19_DisposeReturnsToPool(categoryId),
        Q20_BeginTransactionReturn(categoryId),
        Q21_ReaderForwardOnly(categoryId),
        Q22_OutputParameter(categoryId),
        Q23_AsyncExecution(categoryId),
        Q24_DataTable(categoryId),
        Q25_IsolationLevel(categoryId),
        Q26_CommandTimeout(categoryId),
        Q27_ConnectionStringBuilder(categoryId),
        Q28_DbNullValue(categoryId),
        Q29_CommandBehaviorCloseConnection(categoryId),
        Q30_MultipleResultSets(categoryId),
    ];

    private static Question Q01_OpenConnection(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which `DbConnection` method establishes the connection to the database using its `ConnectionString`?",
            explanation:
                "`Open()` opens the physical (or pooled) connection using the configured `ConnectionString`. " +
                "`Close()`/`Dispose()` release it, and `BeginTransaction()` starts a transaction on an " +
                "already-open connection.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Open()",             isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Connect()",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "BeginTransaction()", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Dispose()",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_ExecuteScalar(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which `DbCommand` method is best for a `SELECT COUNT(*)` that returns a single value?",
            explanation:
                "`ExecuteScalar()` returns the first column of the first row — ideal for aggregates like " +
                "`COUNT`/`SUM`. `ExecuteReader` returns a rowset, `ExecuteNonQuery` returns an affected-row " +
                "count for DML, and there is no `ExecuteAggregate`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "ExecuteScalar()",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "ExecuteReader()",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "ExecuteNonQuery()", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "ExecuteAggregate()", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_ExecuteNonQuery(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which `DbCommand` method executes an `INSERT`, `UPDATE`, or `DELETE` and returns the number of affected rows?",
            explanation:
                "`ExecuteNonQuery()` runs DML/DDL that returns no rowset and gives back the count of affected " +
                "rows. `ExecuteReader` is for `SELECT` rows, `ExecuteScalar` returns a single value, and " +
                "`ExecuteXmlReader` (SQL Server only) returns XML.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "ExecuteNonQuery()",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "ExecuteReader()",    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "ExecuteScalar()",    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "ExecuteXmlReader()", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_ExecuteReader(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which `DbCommand` method runs a `SELECT *` and returns a `DbDataReader` over the result rows?",
            explanation:
                "`ExecuteReader()` returns a `DbDataReader` you iterate with `Read()`. `ExecuteScalar` returns " +
                "only one value, `ExecuteNonQuery` returns an affected-row count, and `Fill` belongs to " +
                "`DbDataAdapter`, not `DbCommand`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "ExecuteReader()",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "ExecuteScalar()",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "ExecuteNonQuery()", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Fill()",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_DataReaderCursor(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "How does a `DbDataReader` expose query results?",
            explanation:
                "A `DbDataReader` is a forward-only, read-only, server-side cursor: you stream rows one at a " +
                "time with `Read()` and cannot move backwards or edit data through it. The full disconnected, " +
                "editable, in-memory copy is a `DataSet`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "As a forward-only, read-only stream of rows",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "As a fully editable in-memory table",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "As a scrollable cursor you can rewind",         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "As a disconnected copy of the whole database",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_DbConnectionRole(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which abstract class lets you connect to and disconnect from a data store and start transactions on it?",
            explanation:
                "`DbConnection` manages the connection lifecycle (`Open`/`Close`/`Dispose`) and exposes " +
                "`BeginTransaction()`. `DbCommand` runs statements, `DbDataReader` reads rows, and " +
                "`DbTransaction` is the transaction object that `BeginTransaction` returns — not the connection.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "DbConnection",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DbCommand",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "DbDataReader",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "DbTransaction", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_CreateParameter(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which `DbCommand` method creates a new `DbParameter` instance for the command?",
            explanation:
                "`CreateParameter()` returns a provider-specific `DbParameter` you then configure and add to " +
                "`command.Parameters`. `Parameters.Add` attaches a parameter, `Prepare` compiles the command, " +
                "and `CreateCommand` belongs to `DbConnection`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "CreateParameter()", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Parameters.Add()",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Prepare()",         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "CreateCommand()",   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_SqlInjection(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the primary reason to use parameterized commands instead of concatenating user input into SQL text?",
            explanation:
                "Parameters separate code from data, so user input is treated as a value rather than executable " +
                "SQL — the main defence against SQL injection. They can also help plan reuse, but the headline " +
                "reason is security, not avoiding a connection, ORM, or transactions.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They prevent SQL injection by separating data from SQL code", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They remove the need to open a connection",                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They turn ADO.NET into an ORM",                                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They make transactions unnecessary",                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_ReaderRead(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "What does `reader.Read()` do in this loop?\n\n" +
                "```csharp\n" +
                "using var reader = command.ExecuteReader();\n" +
                "while (reader.Read())\n" +
                "{\n" +
                "    Console.WriteLine(reader.GetString(0));\n" +
                "}\n" +
                "```",
            explanation:
                "`Read()` advances the cursor to the next row and returns `true` while a row is available, " +
                "`false` once the rows are exhausted. The reader starts positioned BEFORE the first row, so you " +
                "must call `Read()` before accessing any column; it does not load all rows at once.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Advances to the next row, returning false when none remain", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Loads every row into memory at once",                        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Reads a single column from the current row",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Re-executes the command on each call",                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_DataSetDisconnected(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In ADO.NET's disconnected model, which type holds an in-memory cache of tables you can work with after closing the connection?",
            explanation:
                "A `DataSet` is an in-memory, disconnected cache of one or more `DataTable`s — you load it, " +
                "close the connection, and keep working with the data. `DbDataReader` is connected and " +
                "forward-only, `DbCommand` runs statements, and `DbConnection` is the connection itself.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "DataSet",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DbDataReader", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "DbCommand",    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "DbConnection", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_DataAdapterFill(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which `DbDataAdapter` method runs its select command and populates a `DataSet` (or `DataTable`)?",
            explanation:
                "`Fill()` executes the adapter's `SelectCommand` and loads the results into a `DataSet`/" +
                "`DataTable`. `Update()` pushes pending changes back to the database, `Open()` belongs to the " +
                "connection, and there is no `Load()` on `DbDataAdapter` (it's `DataTable.Load`).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Fill()",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Update()", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Open()",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Load()",   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_UsingDisposes(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Why is a `using` statement the recommended way to work with a `DbConnection`?",
            explanation:
                "`using` calls `Dispose()` deterministically when the block exits — even on exceptions — which " +
                "closes the connection and returns it to the pool. It does not make queries faster, disable " +
                "transactions, or remove the need to open the connection.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It disposes the connection deterministically, even on exceptions", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It makes every query run faster",                                  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It opens the connection automatically so you needn't call Open()",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It disables transactions for safety",                              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_ConnectionPooling(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the main benefit of ADO.NET connection pooling?",
            explanation:
                "Opening a physical database connection is expensive; pooling reuses existing open connections " +
                "(keyed by the connection string) instead of creating a new one each time, cutting that cost. " +
                "It doesn't encrypt traffic, remove the need to close connections, or convert SQL to LINQ.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It reuses open physical connections to avoid the cost of opening new ones", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It encrypts all data sent over the connection",                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It removes the need to ever close a connection",                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It translates SQL into LINQ automatically",                                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_StoredProcedure(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "To call a stored procedure by name through a `DbCommand`, what must you set?\n\n" +
                "```csharp\n" +
                "command.CommandText = \"usp_GetCustomers\";\n" +
                "command.CommandType = ____;\n" +
                "```",
            explanation:
                "Setting `CommandType = CommandType.StoredProcedure` tells ADO.NET that `CommandText` is a " +
                "procedure name to invoke (parameters map to its arguments). The default `CommandType.Text` " +
                "would try to execute the name as a raw SQL statement; `TableDirect` is for OLE DB table names.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "CommandType.StoredProcedure", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "CommandType.Text",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "CommandType.TableDirect",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "CommandType.Procedure",       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_DbProviderFactory(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the purpose of `DbProviderFactory` in ADO.NET?",
            explanation:
                "`DbProviderFactory` creates provider-specific objects (connection, command, parameter, …) " +
                "through a common abstract API, so code can stay provider-agnostic. It is not an ORM, a " +
                "connection pool, or a migration tool.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It creates provider-specific objects via a common abstract API", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It maps tables to entity classes like an ORM",                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It is the connection pool implementation",                       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It applies database schema migrations",                          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_Transaction(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Using an explicit ADO.NET transaction, how do you make several commands succeed or fail as a unit?",
            explanation:
                "You call `connection.BeginTransaction()`, assign the returned `DbTransaction` to each " +
                "command's `Transaction` property, then `Commit()` on success or `Rollback()` on error. Simply " +
                "running commands on one connection, opening multiple connections, or setting a `CommandTimeout` " +
                "does not make them atomic.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Begin a transaction, assign it to each command, then Commit or Rollback", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Run all commands on the same connection — they are atomic by default",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Open a separate connection per command",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Set a CommandTimeout on every command",                                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_SqlCommandProvider(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How does `SqlCommand` relate to `DbCommand`?",
            explanation:
                "`SqlCommand` is the SQL Server-specific implementation that derives from the abstract " +
                "`DbCommand` base class (other providers supply `NpgsqlCommand`, `OleDbCommand`, etc.). It is " +
                "not the abstract base itself, not unrelated, and not an interface.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "SqlCommand is a provider-specific subclass of DbCommand", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "DbCommand is a subclass of SqlCommand",                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are unrelated types",                                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "DbCommand is an interface SqlCommand does not implement",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_ExecuteScalarReturn(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What does `ExecuteScalar()` return, and what must callers account for?",
            explanation:
                "`ExecuteScalar()` returns the first column of the first row as `object`, so you cast it to the " +
                "expected type — and it returns `null` when the result set is empty (and `DBNull.Value` for a " +
                "SQL NULL). It does not return a strongly typed value, a row count, or a reader.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The first column of the first row as object (null if no rows)", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A strongly typed value that never needs casting",               isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The number of rows affected",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A DbDataReader over all matching rows",                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_DisposeReturnsToPool(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What happens to a pooled `DbConnection` when you call `Close()` or `Dispose()` on it?",
            explanation:
                "With pooling enabled, `Close()`/`Dispose()` doesn't tear down the physical connection — it " +
                "returns it to the pool for reuse. Forgetting to close connections is what exhausts the pool and " +
                "causes timeouts; the underlying socket is not necessarily destroyed each time.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It is returned to the connection pool for reuse",     isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The physical socket is always destroyed immediately", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "All pending transactions are committed",              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The connection string is cleared from memory",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_BeginTransactionReturn(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does `DbConnection.BeginTransaction()` return?",
            explanation:
                "`BeginTransaction()` returns a `DbTransaction` object, which you assign to each command's " +
                "`Transaction` property and later `Commit()` or `Rollback()`. It does not return a command, a " +
                "reader, or a boolean.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A DbTransaction", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A DbCommand",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A DbDataReader",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A bool indicating success", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q21_ReaderForwardOnly(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How does a DbDataReader traverse a result set?",
            explanation:
                "A DbDataReader is a forward-only, read-only cursor: it streams rows one at a time from the " +
                "server and cannot go back or update data. This makes it fast and memory-light for large " +
                "result sets, but you can only move forward with Read(). For random access or editing, load " +
                "into a DataTable/DataSet instead.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Forward-only and read-only, streaming one row at a time", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Bidirectional, allowing movement back and forth",         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Random access by row index",                              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It loads the entire result set into memory up front",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q22_OutputParameter(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How do you read a value returned via a stored procedure's OUTPUT parameter in ADO.NET?",
            explanation:
                "Add a DbParameter, set its `Direction` to `ParameterDirection.Output` (or InputOutput), and " +
                "after `ExecuteNonQuery` returns, read the parameter's `.Value`. Output parameter values are " +
                "only populated once command execution completes.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Set the parameter's Direction to Output, then read its Value after executing", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Read it from the return value of ExecuteNonQuery",                             isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Output parameters can only be read through a DbDataReader",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Call CreateParameter with Direction Input and read Value",                     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q23_AsyncExecution(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the benefit of the async ADO.NET methods such as `ExecuteReaderAsync` and `OpenAsync`?",
            explanation:
                "Async methods free the calling thread while waiting on I/O (network round-trips to the " +
                "database) instead of blocking it. This improves scalability on servers — the thread can serve " +
                "other requests during the wait. They don't make a single query run faster; they improve " +
                "throughput under load.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They release the thread during I/O waits, improving server scalability", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They make each individual query execute faster on the server",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They automatically retry failed queries",                               isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They encrypt the connection by default",                                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q24_DataTable(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In the disconnected model, what is a DataTable?",
            explanation:
                "A DataTable is an in-memory representation of a table — rows and columns held in memory, " +
                "disconnected from the database. It can be filled by a DataAdapter, navigated and edited freely " +
                "(random access, in contrast to a forward-only reader), and changes can later be pushed back " +
                "via the adapter.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "An in-memory, disconnected representation of rows and columns", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A forward-only cursor over the live connection",               isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A physical table created on the database server",              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A connection-pool data structure",                             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q25_IsolationLevel(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What does a transaction's IsolationLevel control?",
            explanation:
                "Isolation level governs how/when one transaction sees changes made by others — trading " +
                "consistency against concurrency. Higher levels (e.g. Serializable) prevent phenomena like " +
                "dirty reads, non-repeatable reads, and phantom reads, but reduce concurrency; lower levels " +
                "(e.g. ReadCommitted, ReadUncommitted) allow more concurrency with weaker guarantees.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "How much one transaction is affected by concurrent transactions' uncommitted/committed changes", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The maximum number of rows a transaction may modify",                                             isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Whether the transaction uses a pooled connection",                                                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The timeout before the transaction is rolled back",                                               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q26_CommandTimeout(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the `CommandTimeout` property on a DbCommand specify?",
            explanation:
                "CommandTimeout is the number of seconds the command waits for execution to complete before " +
                "throwing a timeout exception. It is distinct from the connection timeout (time to establish " +
                "the connection). Setting it to 0 means wait indefinitely.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "How long to wait for the command to execute before timing out", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "How long to wait to open the connection",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "How long a pooled connection stays alive when idle",            isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The maximum number of rows the command may return",             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q27_ConnectionStringBuilder(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Why use a `DbConnectionStringBuilder` (e.g. `SqlConnectionStringBuilder`) instead of concatenating a connection string?",
            explanation:
                "The builder produces a correctly formatted connection string and properly escapes values, " +
                "guarding against connection-string injection when parts come from user input. It also gives " +
                "strongly-typed, named access to keywords instead of error-prone manual string concatenation.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It safely formats and escapes values, preventing connection-string injection", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It encrypts the password inside the connection string",                        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It opens the connection automatically when built",                             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is required for connection pooling to work",                                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q28_DbNullValue(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "When reading a column that may be SQL NULL from a DbDataReader, why check `reader.IsDBNull(i)` rather than comparing to `null`?",
            explanation:
                "A database NULL is represented by the sentinel `DBNull.Value`, not by a CLR `null`. Reading the " +
                "column with a typed getter like `GetInt32` when the value is DBNull throws. So you test " +
                "`IsDBNull(ordinal)` first (or compare the boxed value to `DBNull.Value`) and substitute a " +
                "default. Comparing the boxed object to `null` would be false.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A SQL NULL is DBNull.Value, not CLR null, and typed getters throw on it", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "IsDBNull is just faster than a null comparison",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Comparing to null closes the reader",                                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "There is no difference; both work identically",                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q29_CommandBehaviorCloseConnection(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does `ExecuteReader(CommandBehavior.CloseConnection)` arrange?",
            explanation:
                "With CommandBehavior.CloseConnection, the associated connection is closed automatically when " +
                "the DbDataReader is closed/disposed. This is handy when a method returns a reader to a caller " +
                "that can't see the connection — disposing the reader also releases the connection back to the " +
                "pool.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Closing the reader also closes the underlying connection", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The connection is kept open permanently for reuse",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The reader returns only a single row",                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The command runs inside an implicit transaction",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q30_MultipleResultSets(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "When a command returns multiple result sets, how do you advance a DbDataReader to the next one?",
            explanation:
                "`NextResult()` moves the reader to the next result set (e.g. when a batch or stored procedure " +
                "returns several SELECTs). You loop `Read()` within a result set and call `NextResult()` to step " +
                "to the following one. `Read()` only moves between rows of the current result set.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Call NextResult() to move to the next result set", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Call Read() again, which rolls over automatically", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Re-execute the command for each result set",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Open a second reader on the same command",          isCorrect: false, orderIndex: 3),
            ]);
    }
}
