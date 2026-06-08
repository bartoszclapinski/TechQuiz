using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "C#/.NET" category. Content adapted from the EPAM .NET
/// Fundamentals course, modules 004-010 (Reflection, JSON Serialization, XML
/// Serialization, Threads &amp; Thread Pools, Synchronization, Task Parallel Library,
/// Task-based Asynchronous Programming) graded and practice quizzes — covering runtime
/// reflection, serialization defaults, the threading and synchronization toolkit, the
/// TPL, and async/await.
/// </summary>
/// <remarks>
/// Several source questions were "select TWO" / "select THREE" items. They were rephrased to
/// single-correct to satisfy the <c>MultipleChoice</c> Domain invariant (exactly one correct
/// option per question).
/// </remarks>
public static class CSharpDotNetQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_ReflectionPurpose(categoryId),
        Q02_TypeofVsGetType(categoryId),
        Q03_GetConstructorsDefault(categoryId),
        Q04_MakeGenericType(categoryId),
        Q05_JsonSerializesProperties(categoryId),
        Q06_JsonWriteIndentedDefault(categoryId),
        Q07_JsonIncludeField(categoryId),
        Q08_XmlSerializerCtor(categoryId),
        Q09_JoinUnstartedThread(categoryId),
        Q10_PooledThreadsBackground(categoryId),
        Q11_LockKeyword(categoryId),
        Q12_Semaphore(categoryId),
        Q13_Deadlock(categoryId),
        Q14_InterlockedMethods(categoryId),
        Q15_TplAdvantage(categoryId),
        Q16_NewTaskDoesNotStart(categoryId),
        Q17_ContinuationChain(categoryId),
        Q18_CancelTask(categoryId),
        Q19_AwaitSuspends(categoryId),
        Q20_AsyncReturnType(categoryId),
    ];

    private static Question Q01_ReflectionPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does reflection in .NET let an application do?",
            explanation:
                "Reflection lets an application inspect its own structure and behaviour at runtime — reading " +
                "type metadata, getting/setting field and property values, invoking methods, and building " +
                "objects, all without knowing the types at compile time. It does not compile C#, manage memory, " +
                "or perform garbage collection.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Inspect type metadata and invoke members at runtime", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Compile C# source code into IL",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Allocate and free unmanaged memory",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Trigger garbage collection on demand",                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_TypeofVsGetType(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "You have an object reference `obj` and need the actual runtime type of the instance. Which call do you use?",
            explanation:
                "`obj.GetType()` is an instance method that returns the actual runtime type of the object. " +
                "`typeof(T)` needs a type name known at compile time, not an expression; `Type.GetType(string)` " +
                "resolves a type from its fully qualified name; `nameof(obj)` returns the identifier text, not a Type.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "obj.GetType()",          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "typeof(obj)",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Type.GetType(\"obj\")",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "nameof(obj)",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_GetConstructorsDefault(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "A type has a static constructor, a public parameterless constructor, and a public constructor " +
                "taking a string. What does `type.GetConstructors()` (no `BindingFlags` argument) return?",
            explanation:
                "`GetConstructors()` with no arguments returns only PUBLIC INSTANCE constructors, so it returns " +
                "the two public ones and excludes the static constructor. To include static or non-public " +
                "constructors you must pass explicit `BindingFlags` (and you must combine `Instance`/`Static` " +
                "with `Public`/`NonPublic`, or nothing is returned).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The two public instance constructors", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "All three constructors",               isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Only the static constructor",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "An empty array",                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_MakeGenericType(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Starting from the open generic definition `typeof(List<>)`, how do you build the closed type `List<int>` via reflection?",
            explanation:
                "`typeof(List<>).MakeGenericType(typeof(int))` produces the closed type `List<int>`, which you " +
                "can then pass to `Activator.CreateInstance`. You cannot instantiate the open definition " +
                "`List<>` directly, `GetGenericTypeDefinition()` goes the other way (closed → open), and " +
                "`MakeArrayType()` builds an array type.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "typeof(List<>).MakeGenericType(typeof(int))",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Activator.CreateInstance(typeof(List<>))",         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "typeof(List<>).GetGenericTypeDefinition()",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "typeof(List<>).MakeArrayType(typeof(int))",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_JsonSerializesProperties(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "By default, which members does `System.Text.Json.JsonSerializer` include when serializing an object?",
            explanation:
                "By default `JsonSerializer` serializes only PUBLIC PROPERTIES. Public fields are ignored unless " +
                "you opt them in with `[JsonInclude]`, and private members are never serialized by default.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Public properties only",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Public properties and public fields", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "All public and private members",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Public fields only",                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_JsonWriteIndentedDefault(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "With no `JsonSerializerOptions` supplied, what does the JSON output of `JsonSerializer.Serialize(obj)` look like?",
            explanation:
                "The default `WriteIndented` is `false`, so the serializer produces minified JSON on a single " +
                "line with no extra whitespace. Set `WriteIndented = true` to get pretty-printed, indented output.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Minified — a single line, no extra whitespace",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Indented and pretty-printed across lines",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Base64-encoded",                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "XML rather than JSON",                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_JsonIncludeField(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "You want `JsonSerializer` to include a PUBLIC FIELD in the output. What is required?",
            explanation:
                "Fields are ignored by default; mark the public field with `[JsonInclude]` (or set " +
                "`IncludeFields = true` in options) to serialize it. `[JsonIgnore]` does the opposite, " +
                "`[Serializable]` is for the legacy `BinaryFormatter`, and making the field `readonly` has no " +
                "effect on serialization.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Annotate the public field with [JsonInclude]", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Annotate the field with [JsonIgnore]",         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Mark the class [Serializable]",                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Make the field readonly",                      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_XmlSerializerCtor(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What must a type provide for `System.Xml.Serialization.XmlSerializer` to deserialize it?",
            explanation:
                "`XmlSerializer` creates the instance by calling a PUBLIC PARAMETERLESS constructor, so the type " +
                "must expose one (the implicit default constructor counts, unless you've declared other " +
                "constructors). It serializes public read/write properties and fields; it does not require " +
                "`[Serializable]`, a `[DataContract]`, or `ISerializable`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A public parameterless constructor",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The [Serializable] attribute",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A [DataContract] attribute",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "An implementation of ISerializable",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_JoinUnstartedThread(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "What happens when this code runs?\n\n" +
                "```csharp\n" +
                "var thread = new Thread(() => { /* work */ });\n" +
                "thread.Join();  // note: no Start()\n" +
                "```",
            explanation:
                "Calling `Join()` on a thread that was never started throws `ThreadStateException` — there is no " +
                "running thread to wait for. The thread does not silently no-op, and `Join()` does not implicitly " +
                "start the thread.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A ThreadStateException is thrown",        isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Join() returns immediately, doing nothing", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Join() starts the thread and waits for it", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The calling thread blocks forever",         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_PooledThreadsBackground(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which statement about thread-pool threads is true?",
            explanation:
                "Thread-pool threads are always BACKGROUND threads, so they don't keep the process alive on their " +
                "own. The runtime manages their scheduling: priority changes are effectively ignored, you don't " +
                "control their lifetime, and they are reused across many work items rather than created per task.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They are always background threads",                  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Their priority can be reliably changed by your code", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A new pool thread is created for every work item",    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They keep the process alive until they finish",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_LockKeyword(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text:
                "Which keyword fills the blank to give one thread at a time exclusive access to the critical section?\n\n" +
                "```csharp\n" +
                "object gate = new object();\n" +
                "____ (gate)\n" +
                "{\n" +
                "    // critical section\n" +
                "}\n" +
                "```",
            explanation:
                "`lock` provides mutual exclusion — it's syntactic sugar over `Monitor.Enter`/`Monitor.Exit`. " +
                "`using` disposes an `IDisposable`, `try` begins exception handling, and `fixed` pins a variable " +
                "for unsafe pointer access.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "lock",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "using", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "try",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "fixed", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_Semaphore(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which synchronization construct limits a resource to a fixed NUMBER of concurrent threads (permits) rather than just one?",
            explanation:
                "A `Semaphore` (or `SemaphoreSlim`) maintains a count of permits and allows up to N threads in at " +
                "once. `Monitor`, `Mutex`, and `SpinLock` are all exclusive — only one thread may hold them at a time.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Semaphore", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Monitor",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Mutex",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "SpinLock",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_Deadlock(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "Two threads run A and B concurrently. What is the most likely outcome?\n\n" +
                "```csharp\n" +
                "void A() { lock (l1) { Thread.Sleep(100); lock (l2) { } } }\n" +
                "void B() { lock (l2) { Thread.Sleep(100); lock (l1) { } } }\n" +
                "```",
            explanation:
                "This is a classic deadlock: A holds `l1` and waits for `l2`, while B holds `l2` and waits for " +
                "`l1`. The `Sleep(100)` all but guarantees each thread takes its first lock before reaching the " +
                "second, producing a circular wait — both threads block forever.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The threads deadlock and hang indefinitely",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Both methods complete in either order",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A DeadlockException is thrown automatically",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The runtime resolves it by releasing one lock", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_InterlockedMethods(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which of these is NOT a real method on the `System.Threading.Interlocked` class?",
            explanation:
                "`Interlocked` provides atomic `Increment`, `Decrement`, `Exchange`, `CompareExchange`, `Add`, " +
                "and `Read`. There is no `Interlocked.Swap` — to atomically swap a value you use `Exchange`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Swap",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Increment",       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Exchange",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "CompareExchange", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_TplAdvantage(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is a key advantage of the Task Parallel Library (TPL) over creating and managing `Thread` objects manually?",
            explanation:
                "The TPL handles task scheduling and thread pooling for you — the scheduler queues work onto the " +
                "thread pool automatically. It does not remove the need to handle exceptions, guarantee execution " +
                "order, or dedicate a separate thread to every task (tasks usually share pool threads).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It handles task scheduling and thread pooling automatically", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It removes the need to handle exceptions in async code",       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It guarantees tasks run in the order they were started",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It runs every task on its own dedicated thread",               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_NewTaskDoesNotStart(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which way of obtaining a `Task` creates it WITHOUT starting it, so it never runs unless you call `.Start()`?",
            explanation:
                "`new Task(action)` only constructs the task in the `Created` state; it must be started explicitly " +
                "with `.Start()`. `Task.Run`, `Task.Factory.StartNew`, and `Task.Delay` all return a task that is " +
                "already scheduled and running.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "new Task(() => { ... })",          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Task.Run(() => { ... })",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Task.Factory.StartNew(() => { ... })", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Task.Delay(1000)",                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_ContinuationChain(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "What does this continuation chain print?\n\n" +
                "```csharp\n" +
                "Task.Factory.StartNew<int>(() => 16)\n" +
                "    .ContinueWith<double>(a => Math.Sqrt(a.Result))\n" +
                "    .ContinueWith<double>(a => a.Result * 2)\n" +
                "    .ContinueWith(a => Console.WriteLine(a.Result))\n" +
                "    .Wait();\n" +
                "```",
            explanation:
                "Each continuation feeds the previous result forward: 16 → √16 = 4 → 4 × 2 = 8 → " +
                "`Console.WriteLine(8)` prints 8.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "8",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "16", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "4",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "32", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_CancelTask(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the correct way to cancel a running task with the TPL?",
            explanation:
                "TPL cancellation is COOPERATIVE: you pass a `CancellationToken` to the task and the task body " +
                "periodically checks it (`IsCancellationRequested` / `ThrowIfCancellationRequested`). `Task` has " +
                "no `Cancel()` method, `Thread.Abort` is obsolete and unsafe, and throwing an arbitrary exception " +
                "is not the cancellation protocol.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Pass a CancellationToken and check it inside the task body", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Call Cancel() on the Task object",                           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Call Thread.Abort() to stop it immediately",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Throw an exception from outside the task",                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_AwaitSuspends(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which statement about the `await` operator is correct?",
            explanation:
                "`await` SUSPENDS the enclosing async method until the awaited operation completes, returning " +
                "control to the caller without blocking the thread. It does not block the calling thread, is not " +
                "optional in a method that needs to await, and does not make the method synchronous.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It suspends the async method without blocking the thread", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It blocks the calling thread until the operation finishes", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It makes the method run synchronously",                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is optional and has no effect on control flow",           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_AsyncReturnType(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which return type is NOT valid for an `async` method in C#?",
            explanation:
                "An async method may return `Task`, `Task<T>`, `void` (mainly for event handlers), or a " +
                "task-like type such as `ValueTask`. It cannot return a plain `int` — to produce an int " +
                "asynchronously the signature must be `Task<int>`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "int",       isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Task",      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Task<int>", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "void",      isCorrect: false, orderIndex: 3),
            ]);
    }
}
