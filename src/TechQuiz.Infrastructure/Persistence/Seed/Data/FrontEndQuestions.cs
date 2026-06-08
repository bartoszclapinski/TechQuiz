using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "Front-End" category. Content covers JavaScript and TypeScript
/// fundamentals (equality and type coercion, scope and hoisting, closures, the event loop,
/// promises and async/await, <c>this</c> binding, and TypeScript's type system) plus core
/// HTML and CSS (semantic markup, the box model, specificity, positioning, flexbox, and grid).
/// Front-End sits outside the EPAM .NET Fundamentals track, so questions are authored from
/// canonical language and platform behaviour at a junior/mid interview level.
/// </summary>
/// <remarks>
/// All questions are single-correct to satisfy the <c>MultipleChoice</c> Domain invariant
/// (exactly one correct option per question).
/// </remarks>
public static class FrontEndQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_StrictEquality(categoryId),
        Q02_TypeofNull(categoryId),
        Q03_LetConstVar(categoryId),
        Q04_Hoisting(categoryId),
        Q05_Closures(categoryId),
        Q06_EventLoop(categoryId),
        Q07_PromiseStates(categoryId),
        Q08_AsyncReturnsPromise(categoryId),
        Q09_ThisBinding(categoryId),
        Q10_TypeScriptCompiles(categoryId),
        Q11_Generics(categoryId),
        Q12_TypeNarrowing(categoryId),
        Q13_MapVsForEach(categoryId),
        Q14_SemanticHtml(categoryId),
        Q15_BoxModel(categoryId),
        Q16_Specificity(categoryId),
        Q17_FlexAxes(categoryId),
        Q18_Positioning(categoryId),
        Q19_GridVsFlex(categoryId),
        Q20_DisplayNoneVsVisibility(categoryId),
    ];

    private static Question Q01_StrictEquality(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "In JavaScript, what is the difference between `==` and `===`?",
            explanation:
                "`===` (strict equality) compares both value and type with no coercion, so `0 === ''` is " +
                "false. `==` (loose equality) coerces operands to a common type before comparing, so " +
                "`0 == ''` is true. Best practice is to use `===` to avoid surprising coercion bugs.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`===` compares value and type without coercion; `==` coerces types before comparing", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They are identical; `===` is just a stylistic alias for `==`",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`==` compares references while `===` compares values",                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`===` performs type coercion while `==` does not",                                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_TypeofNull(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does `typeof null` evaluate to in JavaScript?",
            explanation:
                "`typeof null` returns the string \"object\". This is a long-standing bug from the first " +
                "JavaScript implementation that has been kept for backward compatibility. To check for null, " +
                "compare directly with `=== null`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "\"object\"",    isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "\"null\"",      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "\"undefined\"", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "\"boolean\"",   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_LetConstVar(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which statement about `var`, `let`, and `const` is correct?",
            explanation:
                "`let` and `const` are block-scoped, while `var` is function-scoped. `const` prevents " +
                "reassignment of the binding (though the referenced object can still be mutated). `var` " +
                "declarations are also hoisted and initialised to undefined, unlike `let`/`const`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`let` and `const` are block-scoped; `var` is function-scoped", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "All three are block-scoped",                                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`const` makes the referenced object deeply immutable",         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`var` is block-scoped; `let` and `const` are function-scoped", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_Hoisting(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "What does this code print?\n\n```javascript\nconsole.log(x);\nvar x = 5;\n```",
            explanation:
                "`var` declarations are hoisted to the top of their function scope, but only the declaration — " +
                "not the assignment. So at the point of the log, `x` exists but is still `undefined`; the " +
                "assignment `x = 5` runs afterwards. (With `let`/`const` this would instead throw a " +
                "ReferenceError due to the temporal dead zone.)",
            options:
            [
                new Option(Guid.NewGuid(), qid, "undefined",                 isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "5",                         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "ReferenceError: x is not defined", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "null",                      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_Closures(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is a closure in JavaScript?",
            explanation:
                "A closure is a function bundled together with references to its surrounding lexical scope. " +
                "It lets an inner function keep accessing variables from its outer function even after that " +
                "outer function has returned — the basis for private state, factory functions, and many " +
                "callback patterns.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A function that retains access to variables from its lexical scope even after the outer function has returned", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A block of code wrapped in curly braces that runs immediately",                                                isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A way to close a connection or release a resource when done",                                                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A function that has no access to any variables outside its own body",                                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_EventLoop(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "What is the output order of this code?\n\n```javascript\nconsole.log('A');\nsetTimeout(() => console.log('B'), 0);\nPromise.resolve().then(() => console.log('C'));\nconsole.log('D');\n```",
            explanation:
                "Synchronous code runs first: 'A' then 'D'. After the call stack empties, the event loop " +
                "drains the microtask queue (promise callbacks) before the macrotask queue (timers). So the " +
                "promise's 'C' runs before the setTimeout's 'B', giving A, D, C, B.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A, D, C, B", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A, B, C, D", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A, D, B, C", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A, C, D, B", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_PromiseStates(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What are the three states a JavaScript Promise can be in?",
            explanation:
                "A Promise is always in exactly one of three states: pending (initial, not yet settled), " +
                "fulfilled (resolved with a value), or rejected (failed with a reason). Once it transitions " +
                "from pending to fulfilled or rejected, the state is final and cannot change again.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Pending, fulfilled, rejected",   isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Open, running, closed",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Waiting, success, error, retry", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Created, started, completed",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_AsyncReturnsPromise(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does an `async` function always return?",
            explanation:
                "An `async` function always returns a Promise. If you return a plain value, it is wrapped in " +
                "a resolved Promise; if you throw, it returns a rejected Promise. This is why the result of " +
                "calling an async function must be awaited or consumed with `.then()`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "A Promise",                                          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The raw value returned by the function body",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "undefined, unless an explicit return is used",       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A callback function to be invoked later",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_ThisBinding(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "How does an arrow function differ from a regular function with respect to `this`?",
            explanation:
                "A regular function gets its own `this`, determined by how it is called (the call site). An " +
                "arrow function has no own `this` — it captures `this` lexically from the enclosing scope at " +
                "definition time. This is why arrow functions are convenient for callbacks inside methods, " +
                "where you want to keep the outer `this`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "An arrow function captures `this` lexically from its enclosing scope; a regular function's `this` depends on how it is called", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "An arrow function always binds `this` to the global object",                                                                    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "There is no difference; both resolve `this` at the call site",                                                                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A regular function captures `this` lexically; an arrow function rebinds it on every call",                                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_TypeScriptCompiles(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What happens to TypeScript's type annotations when the code runs in a browser?",
            explanation:
                "TypeScript is compiled (transpiled) to plain JavaScript, and all type annotations are erased " +
                "during this step. Types exist only at compile time for tooling and checking; the browser runs " +
                "the emitted JavaScript and has no knowledge of them at runtime.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They are erased at compile time; the browser runs plain JavaScript", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They are enforced at runtime by the browser's type checker",         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are converted into runtime assertions in the output",           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Modern browsers execute TypeScript directly without compilation",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_Generics(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text:
                "What is the main benefit of generics in this TypeScript function?\n\n```typescript\nfunction identity<T>(value: T): T {\n  return value;\n}\n```",
            explanation:
                "Generics let the function work with any type while preserving the relationship between input " +
                "and output. Calling `identity('hi')` returns a `string`, `identity(42)` returns a `number` — " +
                "the compiler infers and tracks the concrete type, giving type safety without losing reusability " +
                "(unlike `any`, which discards type information).",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It preserves the input's type in the return value while staying reusable across types", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It forces every caller to pass a value of type `T` defined globally",                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It is equivalent to typing the parameter as `any`",                                     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It makes the function run faster at runtime",                                           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_TypeNarrowing(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "In this TypeScript code, why can `value.toFixed(2)` be called safely inside the `if` block?\n\n```typescript\nfunction format(value: string | number): string {\n  if (typeof value === 'number') {\n    return value.toFixed(2);\n  }\n  return value;\n}\n```",
            explanation:
                "This is type narrowing. The `typeof value === 'number'` check acts as a type guard: inside the " +
                "`if` block TypeScript narrows the union `string | number` down to `number`, so number-only " +
                "methods like `toFixed` are allowed. After the block, the remaining type is `string`.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The `typeof` check is a type guard that narrows the union to `number` inside the block", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "TypeScript ignores the union and treats every parameter as `any`",                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`toFixed` exists on both `string` and `number`, so no narrowing is needed",              isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The cast happens at runtime because TypeScript checks types when the code executes",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_MapVsForEach(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the key difference between `Array.prototype.map` and `Array.prototype.forEach`?",
            explanation:
                "`map` returns a new array containing the results of calling the callback on each element. " +
                "`forEach` returns undefined and is used purely for side effects (it does not build a new " +
                "array). Use `map` when you want a transformed array, `forEach` when you just want to iterate.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`map` returns a new array of transformed values; `forEach` returns undefined", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`forEach` returns a new array; `map` mutates the original in place",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are interchangeable and both return a new array",                         isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`map` can break out early with `return`; `forEach` cannot iterate at all",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_SemanticHtml(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the main advantage of using semantic HTML elements like `<header>`, `<nav>`, and `<article>` instead of `<div>`?",
            explanation:
                "Semantic elements describe the meaning of their content, which improves accessibility (screen " +
                "readers can navigate landmarks), SEO, and code readability. A `<div>` is a generic, " +
                "meaningless container. Semantic tags render the same visually but convey structure to " +
                "machines and assistive technology.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "They convey meaning/structure, improving accessibility and SEO", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "They render faster than `<div>` elements in the browser",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They come with built-in styling that `<div>` lacks",             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They are required for JavaScript to select elements",            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_BoxModel(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "From innermost to outermost, what are the layers of the CSS box model?",
            explanation:
                "The CSS box model wraps every element in four layers: content (the actual text/image), then " +
                "padding (space inside, around the content), then border, then margin (space outside the " +
                "border, separating it from other elements). Order: content → padding → border → margin.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Content, padding, border, margin", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Content, margin, border, padding", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Padding, content, margin, border", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Margin, border, content, padding", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_Specificity(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which CSS selector has the highest specificity?",
            explanation:
                "Specificity ranks selectors: inline styles > IDs > classes/attributes/pseudo-classes > " +
                "elements/pseudo-elements. An ID selector (`#header`) beats a class (`.title`), which beats an " +
                "element (`p`). The universal selector `*` adds no specificity. (Inline styles and `!important` " +
                "override even these, but among the selectors listed, the ID wins.)",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`#header` (ID selector)",                isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`.title` (class selector)",              isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "`p` (element selector)",                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`*` (universal selector)",               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_FlexAxes(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In a flex container with the default `flex-direction: row`, what do `justify-content` and `align-items` control?",
            explanation:
                "`justify-content` aligns items along the main axis (horizontal when `flex-direction` is row), " +
                "while `align-items` aligns them along the cross axis (vertical in that case). Changing " +
                "`flex-direction` to column swaps which physical direction each axis maps to.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`justify-content` aligns along the main axis (horizontal); `align-items` along the cross axis (vertical)", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`justify-content` aligns vertically; `align-items` aligns horizontally",                                   isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Both align items horizontally, but with different spacing rules",                                          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`justify-content` sets item order; `align-items` sets item size",                                          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_Positioning(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the difference between `position: absolute` and `position: fixed` in CSS?",
            explanation:
                "An absolutely positioned element is placed relative to its nearest positioned ancestor (an " +
                "ancestor with a position other than static), and it scrolls with the page. A fixed element is " +
                "positioned relative to the viewport and stays in place when the page scrolls — useful for " +
                "sticky headers or back-to-top buttons.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`absolute` is positioned relative to the nearest positioned ancestor; `fixed` is relative to the viewport and ignores scrolling", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`absolute` is relative to the viewport; `fixed` is relative to the parent element",                                                isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are identical; `fixed` is a deprecated alias for `absolute`",                                                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`absolute` always stays fixed during scroll; `fixed` scrolls with the document",                                                   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_GridVsFlex(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "When should you reach for CSS Grid instead of Flexbox?",
            explanation:
                "Flexbox is designed for one-dimensional layouts — distributing items along a single row or " +
                "column. CSS Grid is designed for two-dimensional layouts — controlling rows and columns " +
                "simultaneously. Use Grid for page-level/component grids where you need alignment in both axes; " +
                "use Flexbox for linear arrangements like a navbar or a row of buttons.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Grid is for two-dimensional layouts (rows and columns together); Flexbox is for one-dimensional layouts", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Grid is for one-dimensional layouts; Flexbox handles two dimensions",                                      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Grid only works for images while Flexbox only works for text",                                             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "There is no practical difference; they are interchangeable",                                                isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_DisplayNoneVsVisibility(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "What is the difference between `display: none` and `visibility: hidden`?",
            explanation:
                "`display: none` removes the element from the layout entirely — it takes up no space and is not " +
                "rendered. `visibility: hidden` keeps the element in the layout (its space is still reserved) " +
                "but makes it invisible. Both hide the element visually, but only `visibility: hidden` preserves " +
                "the surrounding layout.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`display: none` removes the element and its space; `visibility: hidden` hides it but keeps its space", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`visibility: hidden` removes the element from layout; `display: none` keeps its space reserved",        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Both remove the element from the layout flow entirely",                                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Both keep the element's space but only `display: none` makes it invisible",                             isCorrect: false, orderIndex: 3),
            ]);
    }
}
