using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "ASP.NET Core" category. Content adapted from the EPAM .NET
/// Fundamentals course, modules 015-021 (Introduction, Middleware, Minimal API,
/// Fundamentals/DI/Routing, Web API, Authentication &amp; Authorization) graded quizzes —
/// covering app models, the middleware pipeline, dependency injection, routing, Web API
/// design, and auth.
/// </summary>
/// <remarks>
/// Several source questions were "select TWO" / "select ALL" items. They were rephrased to
/// single-correct to satisfy the <c>MultipleChoice</c> Domain invariant (exactly one correct
/// option per question).
/// </remarks>
public static class AspNetCoreQuestions
{
    public static IReadOnlyList<Question> CreateAll(Guid categoryId) =>
    [
        Q01_DependencyInjectionFeature(categoryId),
        Q02_PipelineMiddleware(categoryId),
        Q03_RazorPages(categoryId),
        Q04_Blazor(categoryId),
        Q05_MapByPath(categoryId),
        Q06_MapWhenPredicate(categoryId),
        Q07_UseRouting(categoryId),
        Q08_ExceptionHandling(categoryId),
        Q09_BuilderServices(categoryId),
        Q10_AddScoped(categoryId),
        Q11_GetServices(categoryId),
        Q12_OptionalRouteParams(categoryId),
        Q13_RouteConstraint(categoryId),
        Q14_MapGet(categoryId),
        Q15_ControllerBase(categoryId),
        Q16_ApiControllerAttribute(categoryId),
        Q17_RouteToken(categoryId),
        Q18_AuthenticationPurpose(categoryId),
        Q19_UseVsAddAuthentication(categoryId),
        Q20_AuthorizeAttribute(categoryId),
    ];

    private static Question Q01_DependencyInjectionFeature(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which ASP.NET Core feature makes configured services available throughout an app?",
            explanation:
                "Dependency Injection is a built-in, first-class feature. Services are registered in the " +
                "DI container during the builder phase and then injected into controllers, middleware, and " +
                "other components. Middleware processes requests, configuration reads settings, logging " +
                "produces diagnostics.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Dependency injection", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Middleware",           isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Configuration",        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Logging",              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q02_PipelineMiddleware(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which component makes up the ASP.NET Core request-handling pipeline?",
            explanation:
                "The pipeline is composed of middleware components; each inspects, modifies, or " +
                "short-circuits the HTTP request as it flows through. Controllers, Views, and Models are " +
                "MVC endpoints the pipeline routes to — not pipeline components themselves.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Middleware components", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Controllers",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Views",                isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Models",               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q03_RazorPages(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which ASP.NET Core model builds page-based UI apps without separate controller classes?",
            explanation:
                "Razor Pages is page-based: each page has a `.cshtml` file and a `PageModel` that handles " +
                "both the request and the UI logic — no separate controllers. MVC uses controllers, Web API " +
                "returns data, and Blazor builds interactive SPAs with C# components.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Razor Pages", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "MVC",         isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Web API",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Blazor",      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q04_Blazor(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which ASP.NET Core technology lets you build interactive client-side web apps using C# instead of JavaScript?",
            explanation:
                "Blazor builds rich interactive UIs in C#. Blazor WebAssembly runs C# in the browser via " +
                "WebAssembly; Blazor Server runs on the server and updates the UI over SignalR. Razor Pages " +
                "renders server-side HTML, SignalR is real-time messaging, MVC is a server-side pattern.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Blazor",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Razor Pages", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "SignalR",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "MVC",         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q05_MapByPath(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which extension method branches the request pipeline based on the request PATH?",
            explanation:
                "`Map` branches the pipeline by URL path (e.g. requests starting with `/dogs` go to one " +
                "branch). `Use` adds intermediate middleware without branching, `Run` is terminal " +
                "middleware, and `MapWhen` branches on a predicate rather than a path.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Map",     isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Use",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Run",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "MapWhen", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q06_MapWhenPredicate(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which extension method branches the pipeline based on an arbitrary predicate, such as the presence of a query-string key?",
            explanation:
                "`MapWhen` branches on a `Func<HttpContext, bool>` predicate — e.g. when the query contains a " +
                "given key. `Map` branches only by URL path, `Run` is terminal and takes no predicate, `Use` " +
                "adds non-branching middleware.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "MapWhen", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Map",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Run",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Use",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q07_UseRouting(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the `UseRouting` middleware add to the pipeline?",
            explanation:
                "`UseRouting` adds route-matching: it examines the request URL and matches it against " +
                "registered route templates to select an endpoint. Payload parsing, auth checks, and logging " +
                "are handled by other middleware.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Route matching functionality",            isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Request parsing and payload handling",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Authentication and authorization checks",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Logging and error handling",               isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q08_ExceptionHandling(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which middleware handles unhandled exceptions in PRODUCTION by showing a user-friendly error page instead of a stack trace?",
            explanation:
                "`UseExceptionHandler` catches unhandled exceptions in production and redirects to a friendly " +
                "error page. `UseDeveloperExceptionPage` shows a detailed stack trace and is for Development " +
                "only; `UseHttpsRedirection` redirects HTTP to HTTPS.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "UseExceptionHandler",          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "UseDeveloperExceptionPage",    isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "UseHttpsRedirection",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "UseStatusCodePages",           isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q09_BuilderServices(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text:
                "Which `WebApplicationBuilder` property registers services in the DI container?\n\n" +
                "```csharp\n" +
                "var builder = WebApplication.CreateBuilder(args);\n" +
                "builder.____.AddSingleton<IMyService, MyService>();\n" +
                "```",
            explanation:
                "`builder.Services` returns the `IServiceCollection` used to register services. " +
                "`Configuration` reads settings, `Host` configures the app host, and `Environment` exposes " +
                "the current environment.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Services",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Configuration", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Host",          isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Environment",   isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q10_AddScoped(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which registration method creates one service instance per client request (scope)?",
            explanation:
                "`AddScoped` creates one instance per HTTP request. `AddTransient` creates a new instance " +
                "every time the service is resolved, and `AddSingleton` creates a single instance for the " +
                "whole application lifetime.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "AddScoped",    isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "AddTransient", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "AddSingleton", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "AddInstance",  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q11_GetServices(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "Three implementations of `IMyService` are registered. What does this print?\n\n" +
                "```csharp\n" +
                "services.AddTransient<IMyService, MyService1>();\n" +
                "services.AddTransient<IMyService, MyService2>();\n" +
                "services.AddTransient<IMyService, MyService3>();\n" +
                "var provider = services.BuildServiceProvider();\n" +
                "Console.WriteLine(provider.GetServices<IMyService>().Count());\n" +
                "```",
            explanation:
                "`GetServices<T>()` (plural) returns ALL registered implementations, so the count is 3. " +
                "`GetService<T>()` (singular) would return only the last-registered implementation.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "3", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "1", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "2", isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "0", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q12_OptionalRouteParams(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which route template maps a GET request with TWO optional parameters, `category` and `page`?",
            explanation:
                "Both parameters need the `?` suffix to be optional: `/products/{category?}/{page?}`. Templates " +
                "with only one `?` make the other parameter required, and the query-string form is a URL, not " +
                "a route template.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "/products/{category?}/{page?}", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "/products/{category?}/{page}",  isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "/products/{category}/{page?}",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "/products?category={category}&page={page}", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q13_RouteConstraint(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Which route template matches `/events/{eventCode}` where `eventCode` must be exactly six alphabetic characters?",
            explanation:
                "Constraints chain with `:` — `length(6)` enforces exactly six characters and `alpha` " +
                "restricts to letters: `{eventCode:length(6):alpha}`. `int` accepts only integers, `length(6)` " +
                "alone ignores character type, and `min(6)` is a numeric-value constraint.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "/events/{eventCode:length(6):alpha}", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "/events/{eventCode:length(6)}",       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "/events/{eventCode:int}",             isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "/events/{eventCode:min(6)}",          isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q14_MapGet(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "Which minimal-API call handles an HTTP GET request to the root path `/` and returns \"Hello, World!\"?",
            explanation:
                "`MapGet(\"/\", () => \"Hello, World!\")` maps a GET on the root path. `MapPost` and `MapPut` " +
                "handle other HTTP verbs, and `Map(\"/hello\", ...)` uses the wrong path.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "app.MapGet(\"/\", () => \"Hello, World!\");",      isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "app.MapPost(\"/\", () => \"Hello, World!\");",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "app.MapPut(\"/\", () => \"Hello, World!\");",      isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "app.Map(\"/hello\", () => \"Hello, World!\");",    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q15_ControllerBase(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which class is recommended as the base class for a Web API controller?",
            explanation:
                "`ControllerBase` provides API helper methods (`Ok()`, `NotFound()`, `BadRequest()`) without " +
                "view support. `Controller` adds MVC view support (unnecessary for APIs), and `[ApiController]` " +
                "is an attribute, not a base class.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "ControllerBase", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Controller",     isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "ApiController",  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "PageModel",      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q16_ApiControllerAttribute(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which attribute on a controller class enables automatic behaviours such as binding-source inference and automatic model-state validation?",
            explanation:
                "`[ApiController]` turns on conventions like binding-source inference (`[FromBody]` for complex " +
                "types, `[FromRoute]`/`[FromQuery]` for simple ones) and automatic 400 responses on invalid " +
                "model state. `[HttpPost]` is an HTTP verb, `[FromRoute]` is a parameter source, `[Authorize]` " +
                "is for authorization.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "[ApiController]", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "[HttpPost]",      isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "[FromRoute]",     isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "[Authorize]",     isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q17_RouteToken(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "Which `[Route]` value maps `ProductsController` to `/api/products`?\n\n" +
                "```csharp\n" +
                "[ApiController]\n" +
                "[Route(____)]\n" +
                "public class ProductsController : ControllerBase { }\n" +
                "```",
            explanation:
                "The `[controller]` token is replaced by the controller name without the \"Controller\" " +
                "suffix, so `\"api/[controller]\"` becomes `/api/products`. `\"api/controller\"` is literal " +
                "text, `\"products\"` omits `/api`, and `\"api/[controller]/products\"` adds an extra segment.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "\"api/[controller]\"",          isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "\"api/controller\"",            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "\"products\"",                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "\"api/[controller]/products\"", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q18_AuthenticationPurpose(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What is the primary purpose of authentication?",
            explanation:
                "Authentication verifies a user's identity (\"who are you?\"). Controlling what a user may do " +
                "is authorization; input validation and XSS prevention are separate security concerns.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "To verify a user's identity",                  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "To grant access to resources based on permissions", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "To validate user inputs",                       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "To prevent cross-site scripting (XSS) attacks", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q19_UseVsAddAuthentication(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Which call adds the authentication MIDDLEWARE to the request pipeline?",
            explanation:
                "Middleware is added with `Use*` on `app`, so `app.UseAuthentication()` adds the authentication " +
                "middleware. `AddAuthentication()` registers services on `builder.Services`, and " +
                "`UseAuthorization()` is the authorization middleware.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "app.UseAuthentication()",  isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "builder.Services.AddAuthentication()", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "app.UseAuthorization()",   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "builder.Services.AddAuthorization()", isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q20_AuthorizeAttribute(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text:
                "Cookie auth is configured with `LoginPath = \"/login\"`. An UNAUTHENTICATED user requests " +
                "`/hello`, which is marked `[Authorize]`. What response do they get?",
            explanation:
                "`[Authorize]` triggers a challenge for the unauthenticated user; cookie authentication " +
                "redirects to its `LoginPath` (`/login`), so the `/login` endpoint's response (\"Login\") is " +
                "returned. With JWT bearer (no LoginPath) the result would instead be 401 Unauthorized.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The response from the /login endpoint", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The response from the /hello endpoint", isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The home page at /",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "An immediate 401 Unauthorized",         isCorrect: false, orderIndex: 3),
            ]);
    }
}
