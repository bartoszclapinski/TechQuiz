using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "Design Patterns" category. Content covers the EPAM .NET
/// Fundamentals course, module 014 (Design Patterns &amp; Application Architecture) — the
/// GoF creational, structural, and behavioral patterns, the SOLID principles, and common
/// architectural styles. Module 014 ships topic notes rather than quizzes, so questions are
/// authored from the canonical pattern and principle definitions.
/// </summary>
/// <remarks>
/// All questions are single-correct to satisfy the <c>MultipleChoice</c> Domain invariant
/// (exactly one correct option per question).
/// </remarks>
public static class DesignPatternsQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_Singleton(categoryId),
        Q02_FactoryMethod(categoryId),
        Q03_AbstractFactory(categoryId),
        Q04_Builder(categoryId),
        Q05_Adapter(categoryId),
        Q06_Decorator(categoryId),
        Q07_Composite(categoryId),
        Q08_Bridge(categoryId),
        Q09_Strategy(categoryId),
        Q10_Observer(categoryId),
        Q11_Visitor(categoryId),
        Q12_TemplateMethod(categoryId),
        Q13_Iterator(categoryId),
        Q14_Command(categoryId),
        Q15_Srp(categoryId),
        Q16_Ocp(categoryId),
        Q17_Lsp(categoryId),
        Q18_Isp(categoryId),
        Q19_Dip(categoryId),
        Q20_OnionArchitecture(categoryId),
    ];

    private static Question Q01_Singleton(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which design pattern ensures a class has only one instance and provides a global point of access to it?",
            explanation:
                "Singleton restricts instantiation to a single object and exposes it globally. Factory Method " +
                "creates objects via subclasses, Prototype clones existing objects, and Builder assembles a " +
                "complex object step by step.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Singleton",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Factory Method", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Prototype",      isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Builder",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_FactoryMethod(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern defines an interface for creating an object but lets subclasses decide which concrete class to instantiate?",
            explanation:
                "Factory Method defers the choice of concrete type to subclasses that override the creation " +
                "method. Abstract Factory creates whole families of related objects, Singleton controls " +
                "instance count, and Adapter converts interfaces.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Factory Method",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Abstract Factory", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Singleton",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Adapter",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_AbstractFactory(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern provides an interface for creating FAMILIES of related or dependent objects without specifying their concrete classes?",
            explanation:
                "Abstract Factory groups multiple factory methods to produce a consistent family of products " +
                "(e.g. matching UI widgets per theme). Factory Method creates a single product, Builder " +
                "assembles one complex object, and Facade simplifies a subsystem's interface.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Abstract Factory", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Factory Method",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Builder",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Facade",           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_Builder(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern separates the construction of a complex object from its representation, building it step by step?",
            explanation:
                "Builder constructs an object incrementally, letting the same process yield different " +
                "representations. Prototype copies an existing instance, Singleton limits instances, and " +
                "Composite arranges objects into trees.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Builder",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Prototype", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Singleton", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Composite", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_Adapter(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which structural pattern converts the interface of a class into another interface that clients expect, letting incompatible types work together?",
            explanation:
                "Adapter wraps an existing class to present the interface a client needs — the classic " +
                "'plug converter'. Decorator adds behaviour, Proxy controls access, and Bridge separates an " +
                "abstraction from its implementation.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Adapter",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Decorator", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Proxy",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Bridge",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_Decorator(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern attaches additional responsibilities to an object dynamically by wrapping it, as a flexible alternative to subclassing?",
            explanation:
                "Decorator wraps an object in another object of a compatible interface to add behaviour at " +
                "runtime (e.g. .NET stream wrappers). Adapter changes an interface rather than adding behaviour, " +
                "Composite builds trees, and Strategy swaps algorithms.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Decorator", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Adapter",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Composite", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Strategy",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_Composite(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern composes objects into tree structures so that clients can treat individual objects and compositions uniformly?",
            explanation:
                "Composite models part-whole hierarchies (e.g. files and folders) behind one interface, so " +
                "leaves and containers are used the same way. Decorator adds behaviour, Bridge splits " +
                "abstraction from implementation, and Flyweight shares fine-grained objects.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Composite", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Decorator", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Bridge",    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Flyweight", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_Bridge(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which pattern decouples an abstraction from its implementation so that the two can vary independently?",
            explanation:
                "Bridge splits a hierarchy into an abstraction and a separate implementation, composed by " +
                "reference, so each side evolves on its own (avoiding a combinatorial subclass explosion). " +
                "Adapter retrofits one existing interface, Strategy swaps algorithms behind one abstraction, " +
                "and Decorator layers behaviour.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Bridge",    isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Adapter",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Strategy",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Decorator", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_Strategy(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which behavioral pattern defines a family of interchangeable algorithms, encapsulates each one, and lets the algorithm vary independently of clients that use it?",
            explanation:
                "Strategy puts each algorithm behind a common interface so it can be selected and swapped at " +
                "runtime. State changes behaviour by internal state, Template Method fixes a skeleton in a base " +
                "class, and Observer handles notifications.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Strategy",        isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "State",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Template Method", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Observer",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_Observer(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern defines a one-to-many dependency so that when one object changes state, all its dependents are notified automatically?",
            explanation:
                "Observer lets a subject publish state changes to any number of subscribers — the model behind " +
                ".NET events and `IObservable<T>`. Mediator centralizes peer communication, Visitor adds " +
                "operations to a structure, and Command encapsulates a request.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Observer", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Mediator", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Visitor",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Command",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_Visitor(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which pattern lets you add new operations to a set of object types WITHOUT modifying those types, by moving the operation into a separate object?",
            explanation:
                "Visitor externalizes operations: each element accepts a visitor that carries the new behaviour, " +
                "so you add operations without touching the element classes (at the cost of making it harder to " +
                "add new element types). Strategy swaps one algorithm, Decorator wraps behaviour, and Iterator " +
                "traverses a collection.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Visitor",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Strategy",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Decorator", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Iterator",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_TemplateMethod(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern defines the skeleton of an algorithm in a base method, deferring some steps to subclasses without changing the algorithm's overall structure?",
            explanation:
                "Template Method fixes the invariant steps in a base class and lets subclasses override the " +
                "variable steps (hooks). Strategy composes interchangeable algorithms by delegation instead of " +
                "inheritance, Builder assembles objects, and Factory Method creates them.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Template Method", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Strategy",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Builder",         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Factory Method",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_Iterator(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which pattern provides sequential access to the elements of a collection without exposing its underlying representation?",
            explanation:
                "Iterator gives a uniform way to traverse a collection — in .NET, `IEnumerator`/`IEnumerable` " +
                "and `foreach`. Composite builds trees, Observer notifies subscribers, and Mediator coordinates " +
                "objects.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Iterator",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Composite", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Observer",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Mediator",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_Command(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which pattern encapsulates a request as an object, letting you parameterize clients, queue or log requests, and support undo?",
            explanation:
                "Command turns an action plus its parameters into an object, decoupling the invoker from the " +
                "receiver and enabling queuing, logging, and undo/redo. Strategy encapsulates an algorithm, " +
                "Observer notifies subscribers, and Memento captures state snapshots.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Command",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Strategy", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Observer", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Memento",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_Srp(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the Single Responsibility Principle (the 'S' in SOLID) state?",
            explanation:
                "SRP says a class should have only one reason to change — one responsibility or actor it serves. " +
                "The other options describe OCP (open/closed), DIP (depend on abstractions), and ISP (small " +
                "focused interfaces).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A class should have only one reason to change",                  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Classes should be open for extension but closed for modification", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "High-level modules should depend on abstractions",                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Interfaces should be small and client-specific",                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_Ocp(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "The Open/Closed Principle states that software entities should be...",
            explanation:
                "OCP: entities should be OPEN for extension but CLOSED for modification — you add new behaviour " +
                "by extending (new types, polymorphism), not by editing existing, tested code. The other " +
                "options paraphrase SRP, LSP, and ISP.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Open for extension, but closed for modification",     isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Responsible for exactly one thing",                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Substitutable for their base types",                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Split into many small, role-specific interfaces",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_Lsp(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which SOLID principle is violated when a subclass overrides a method in a way that breaks the expectations callers have of the base type?",
            explanation:
                "The Liskov Substitution Principle requires that objects of a subtype be usable anywhere the base " +
                "type is expected, without breaking correctness — the classic Square/Rectangle problem. SRP is " +
                "about responsibilities, ISP about interface size, and DIP about dependency direction.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Liskov Substitution Principle", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Single Responsibility Principle", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Interface Segregation Principle", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Dependency Inversion Principle",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_Isp(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the Interface Segregation Principle recommend?",
            explanation:
                "ISP says no client should be forced to depend on methods it does not use — prefer several " +
                "small, role-specific interfaces over one large 'fat' interface. The distractors restate SRP, " +
                "OCP, and DIP.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Prefer many small, client-specific interfaces over one large one", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A class should have a single responsibility",                      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Code should be open for extension, closed for modification",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Depend on abstractions, not concrete implementations",             isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_Dip(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which statement best captures the Dependency Inversion Principle?",
            explanation:
                "DIP: high-level modules and low-level modules should both depend on ABSTRACTIONS, not on each " +
                "other directly, and abstractions should not depend on details. This is what makes DI and " +
                "Clean/Onion architectures possible. The distractors invert the rule or confuse it with " +
                "dependency injection mechanics or OCP.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "High- and low-level modules should both depend on abstractions",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "High-level modules should depend directly on low-level modules",    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Every dependency must be passed through a constructor",             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Classes should be open for extension but closed for modification",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_OnionArchitecture(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "In Onion / Clean Architecture, in which direction do source-code dependencies point?",
            explanation:
                "Dependencies point INWARD: outer layers (infrastructure, UI) depend on inner layers, and the " +
                "domain core at the centre depends on nothing external. Outer concerns are reached through " +
                "abstractions defined inside (dependency inversion), which is why the domain stays framework- " +
                "and database-agnostic.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Inward, toward the domain core",                       isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Outward, from the domain toward infrastructure",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Both directions equally",                               isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "From the UI directly to the database layer",            isCorrect: false, orderIndex: 3),
            ]);
    }
}
