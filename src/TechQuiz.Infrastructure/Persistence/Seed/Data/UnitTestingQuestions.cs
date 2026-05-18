using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "Unit Testing" category. Content adapted from the EPAM .NET
/// Fundamentals course, module 003 (Unit Testing) — 14 questions from the module-end
/// quiz and 5 from the test-doubles mini-quiz.
/// </summary>
/// <remarks>
/// One source question ("Which THREE frameworks…") had three correct options and was
/// rephrased to a NOT-question with a single correct answer to satisfy the
/// <c>MultipleChoice</c> Domain invariant (exactly one correct option).
/// </remarks>
public static class UnitTestingQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_MainPurpose(categoryId),
        Q02_NotAFramework(categoryId),
        Q03_MsTestTestMethod(categoryId),
        Q04_NUnitTestAttribute(categoryId),
        Q05_NUnitTestCase(categoryId),
        Q06_AaaAcronym(categoryId),
        Q07_NUnitAssertThat(categoryId),
        Q08_CorrectStatementAboutUnitTests(categoryId),
        Q09_AssertPhasePurpose(categoryId),
        Q10_NUnitSetUp(categoryId),
        Q11_NUnitTestFixture(categoryId),
        Q12_XUnitFact(categoryId),
        Q13_XUnitInlineData(categoryId),
        Q14_NUnitTearDown(categoryId),
        Q15_NotATestDouble(categoryId),
        Q16_SystemUnderTest(categoryId),
        Q17_StubFunction(categoryId),
        Q18_MoqReturns(categoryId),
        Q19_MoqTimesOnce(categoryId),
    ];

    private static Question Q01_MainPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the main purpose of unit testing?",
            explanation:
                "Unit tests verify that isolated portions of code work correctly under specific " +
                "conditions. They do not rely on databases, UI, or external systems.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To verify the correctness of individual units of code", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To test UI responsiveness",                              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To measure application performance",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To test an entire application as a whole",              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_NotAFramework(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which framework is NOT commonly used for unit testing in C#?",
            explanation:
                "MSTest, NUnit, and xUnit are the three main unit testing frameworks for C# / .NET. " +
                "JUnit is the canonical Java unit testing framework.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "MSTest", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "NUnit",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "xUnit",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "JUnit",  isCorrect: true,  orderIndex: 3),
            ]);
    }

    private static Question Q03_MsTestTestMethod(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In MSTest, which attribute is used to define a test method?",
            explanation:
                "In MSTest, test methods must be marked with `[TestMethod]`, which tells the " +
                "runner that the method is a unit test and should be executed.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[UnitTest]",   isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[Test]",       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Fact]",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[TestMethod]", isCorrect: true,  orderIndex: 3),
            ]);
    }

    private static Question Q04_NUnitTestAttribute(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In NUnit, which attribute is used to mark a test method?",
            explanation:
                "`[Test]` marks a method as an NUnit test. `[TestFixture]` marks the containing " +
                "class, `[Fact]` is the xUnit equivalent, `[TestMethod]` is MSTest.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[TestFixture]", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[Test]",        isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Fact]",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[TestMethod]",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_NUnitTestCase(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In NUnit, which attribute allows multiple sets of data to be passed to a test method?",
            explanation:
                "`[TestCase]` provides parameterised inputs to a single NUnit test method. " +
                "`[InlineData]` is the xUnit equivalent (used with `[Theory]`).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[InlineData]",  isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[TestCase]",    isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[TestFixture]", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[Fact]",        isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_AaaAcronym(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "In unit testing, what does AAA stand for?",
            explanation:
                "Arrange → set up objects and inputs. Act → call the method under test. " +
                "Assert → verify the outcome.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Arrange-Assign-Assess", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Arrange-Act-Assert",    isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Assign-Act-Assert",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Analyze-Act-Assert",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_NUnitAssertThat(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "In NUnit, which assertion correctly verifies the result of `Calculator.Sum(4, 3)`?\n\n" +
                "```csharp\n" +
                "[Test]\n" +
                "public void Sum_ShouldReturnCorrectResult()\n" +
                "{\n" +
                "    int result = Calculator.Sum(4, 3);\n" +
                "    // ???\n" +
                "}\n" +
                "```",
            explanation:
                "NUnit's constraint-based syntax: `Assert.That(actual, Is.EqualTo(expected))`. " +
                "4 + 3 = 7, so the expected value is 7.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Assert.Fail();",                          isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Assert.That(result, Is.Not.EqualTo(7));", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Assert.That(result, Is.EqualTo(5));",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Assert.That(result, Is.EqualTo(7));",     isCorrect: true,  orderIndex: 3),
            ]);
    }

    private static Question Q08_CorrectStatementAboutUnitTests(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which of the following statements about unit tests is correct?",
            explanation:
                "Unit tests must be independent (each test stands on its own and can run in any " +
                "order), isolated (no real database or external systems), and focused on a single " +
                "unit of code.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They should be independent and test only a single unit of code", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They should interact with a real database",                      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They should be dependent on the execution order of other tests", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They should cover the entire application",                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_AssertPhasePurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the purpose of the Assert phase in the Arrange-Act-Assert pattern?",
            explanation:
                "Assert verifies the outcome of the executed code. Preparing data is Arrange, " +
                "calling the method is Act, and cleanup happens outside the AAA pattern (e.g. " +
                "in `[TearDown]` for NUnit).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To execute the method being tested",  isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To verify the outcome of the executed code", isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To clean up resources after a test",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To prepare test data before execution", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_NUnitSetUp(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In NUnit, what is the purpose of the `[SetUp]` attribute?",
            explanation:
                "`[SetUp]` runs before each individual test in the fixture — useful for " +
                "re-initialising shared state. Post-test cleanup is `[TearDown]`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To clean up resources after test execution",   isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To mark a method as a test case",              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To define setup logic that will run before each test", isCorrect: true,  orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To mark a class as a test suite",              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_NUnitTestFixture(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In NUnit, which of the following attributes marks a class as a test class?",
            explanation:
                "`[TestFixture]` marks the containing class as an NUnit test suite. `[Test]` " +
                "marks individual methods within that class.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[Test]",        isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[TestFixture]", isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Ignore]",      isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[SetUp]",       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_XUnitFact(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In xUnit, which of the following attributes is used to define a test method?",
            explanation:
                "`[Fact]` marks an xUnit test method that takes no parameters. `[Theory]` + " +
                "`[InlineData]` is used for parameterised tests.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[Fact]",       isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[TestMethod]", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Test]",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[UnitTest]",   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_XUnitInlineData(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In xUnit, which of the following attributes allows multiple sets of data to be passed to a test method?",
            explanation:
                "`[InlineData]` provides parameter values to a `[Theory]`-annotated test method. " +
                "`[TestCase]` is the NUnit equivalent.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[TestCase]",    isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[TestFixture]", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[Fact]",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[InlineData]",  isCorrect: true,  orderIndex: 3),
            ]);
    }

    private static Question Q14_NUnitTearDown(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "In NUnit, which attribute should be placed before the `Cleanup` method so that it runs after each test?\n\n" +
                "```csharp\n" +
                "public class CalculatorTests\n" +
                "{\n" +
                "    [Test]\n" +
                "    public void SampleTest() {}\n\n" +
                "    // ???\n" +
                "    public void Cleanup()\n" +
                "    {\n" +
                "        // Cleanup after each test.\n" +
                "    }\n" +
                "}\n" +
                "```",
            explanation:
                "`[TearDown]` runs after each test in the fixture — the mirror of `[SetUp]`. " +
                "Use `[OneTimeTearDown]` for fixture-level cleanup that runs once after all tests.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[TearDown]",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[PostTest]",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[CleanUp]",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[AfterTest]", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_NotATestDouble(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which of the following is NOT a type of test double?",
            explanation:
                "The four classic test-double types are Mock, Stub, Dummy, and Fake. Wrapper is " +
                "a general design-pattern term, not a test-double category.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Mock",    isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Dummy",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Wrapper", isCorrect: true,  orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Stub",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_SystemUnderTest(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the term \"system under test\" (SUT) refer to?",
            explanation:
                "SUT is the component, class, or method that the test is actually exercising. " +
                "Surrounding dependencies are typically replaced with test doubles.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The component, class, or method being tested",         isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The set of dependencies required to test an object",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The testing framework being used",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The entire application, regardless of the test type", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_StubFunction(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which of the following best describes the function of a stub in test doubles?",
            explanation:
                "A stub returns predefined values to feed the SUT during a test. Mocks track " +
                "interactions, dummies satisfy parameter requirements without logic, and fakes are " +
                "working but simplified implementations.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Used for compilation only; contains no logic",     isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A full implementation that mimics the real component", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Tracks which methods were called and how often",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Returns fixed values for testing",                isCorrect: true,  orderIndex: 3),
            ]);
    }

    private static Question Q18_MoqReturns(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "Which Moq method should be used after `Setup` to define a return value?\n\n" +
                "```csharp\n" +
                "var mock = new Mock<IMyInterface>();\n" +
                "mock.Setup(x => x.GetValue()).____(42);\n" +
                "```",
            explanation: "Moq fluent syntax: `mock.Setup(x => x.GetValue()).Returns(42);`",
            options:
            [
                new Option(Guid.NewGuid(), qid, "SetResult", isCorrect: false, orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Returns",   isCorrect: true,  orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Resolve",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Responds",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_MoqTimesOnce(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "Which Moq option verifies that the mocked `SaveData` method was called exactly once?\n\n" +
                "```csharp\n" +
                "var mock = new Mock<IDataService>();\n" +
                "mock.Setup(x => x.SaveData());\n" +
                "mock.Object.SaveData();\n" +
                "mock.Verify(x => x.SaveData(), _______);\n" +
                "```",
            explanation: "`Times.Once` (or `Times.Exactly(1)`) asserts a single invocation: `mock.Verify(x => x.SaveData(), Times.Once);`",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Times.Once",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Called.Once()",   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Assert.Equal(1)", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "OneTime()",       isCorrect: false, orderIndex: 3),
            ]);
    }
}
