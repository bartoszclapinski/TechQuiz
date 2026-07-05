using TechQuiz.Domain;

namespace TechQuiz.Infrastructure.Persistence.Seed.Data;

/// <summary>
/// Question bank for the "ASP.NET Core" category (.NET track). Covers app models, the
/// middleware pipeline, dependency injection, routing, minimal APIs, Web API design, and
/// authentication &amp; authorization.
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
        Q21_RunTerminal(categoryId),
        Q22_ServiceLifetimes(categoryId),
        Q23_FromBodyBinding(categoryId),
        Q24_IActionResult(categoryId),
        Q25_ConfigurationSystem(categoryId),
        Q26_IOptionsPattern(categoryId),
        Q27_Cors(categoryId),
        Q28_ModelValidation(categoryId),
        Q29_AllowAnonymous(categoryId),
        Q30_MiddlewareOrder(categoryId),
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

    private static Question Q21_RunTerminal(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What is the difference between `app.Use(...)` and `app.Run(...)` in the middleware pipeline?",
            explanation:
                "`Use` adds middleware that can call `next()` to pass the request further down the pipeline. " +
                "`Run` adds terminal middleware that does not call the next delegate — it ends the pipeline and " +
                "produces a response. `Run` is typically the last middleware registered.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "`Use` can call the next middleware; `Run` is terminal and ends the pipeline", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "`Run` can call the next middleware; `Use` is always terminal",               isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "They are identical; `Run` is a deprecated alias for `Use`",                  isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "`Use` registers services; `Run` registers middleware",                      isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q22_ServiceLifetimes(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "How does a Singleton service lifetime differ from Scoped and Transient in ASP.NET Core DI?",
            explanation:
                "Singleton: one instance for the whole application lifetime. Scoped: one instance per request " +
                "(scope). Transient: a new instance every time it is resolved. A common pitfall is injecting a " +
                "Scoped service into a Singleton — the Scoped instance would be captured for the app's lifetime.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Singleton: one per app; Scoped: one per request; Transient: a new one each resolution", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Singleton: one per request; Scoped: one per app; Transient: never created",             isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "All three create a new instance on every request",                                       isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "They differ only in performance, not in instance count",                                  isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q23_FromBodyBinding(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In a Web API controller, what does the `[FromBody]` attribute tell model binding?",
            explanation:
                "`[FromBody]` instructs the binder to read the parameter's value from the HTTP request body " +
                "(typically deserialized from JSON). Other sources have their own attributes: `[FromQuery]`, " +
                "`[FromRoute]`, `[FromHeader]`, `[FromForm]`. With `[ApiController]`, complex types are bound " +
                "from the body by convention.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Bind the parameter from the request body (e.g. deserialized JSON)", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Bind the parameter from the query string",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Bind the parameter from a route segment",                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Bind the parameter from an HTTP header",                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q24_IActionResult(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "Why might a controller action return `IActionResult` rather than a concrete model type?",
            explanation:
                "Returning `IActionResult` lets one action return different HTTP responses depending on the " +
                "outcome — `Ok(data)` (200), `NotFound()` (404), `BadRequest()` (400), `CreatedAtAction(...)` " +
                "(201), etc. A concrete return type can only express the success payload, not varying status " +
                "codes.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "So the action can return different HTTP results/status codes (Ok, NotFound, BadRequest, …)", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Because EF Core requires it for database access",                                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Because concrete types cannot be serialized to JSON",                                        isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It is required for an action to be asynchronous",                                            isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q25_ConfigurationSystem(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In ASP.NET Core, if the same configuration key is defined in both appsettings.json and an environment variable, which wins by default?",
            explanation:
                "Configuration providers are layered and later providers override earlier ones. The default " +
                "host adds appsettings.json, then appsettings.{Environment}.json, then user secrets (Dev), then " +
                "environment variables, then command-line args. So an environment variable overrides the same " +
                "key from appsettings.json.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The environment variable, because it is added by a later provider", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "appsettings.json always takes precedence over everything",          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It throws an error because of the duplicate key",                   isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The values are concatenated together",                              isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q26_IOptionsPattern(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What does the Options pattern (`IOptions<T>`) provide in ASP.NET Core?",
            explanation:
                "The Options pattern binds a section of configuration to a strongly-typed POCO and exposes it " +
                "through DI as `IOptions<T>` (or `IOptionsSnapshot<T>`/`IOptionsMonitor<T>` for reloadable " +
                "values). It gives type-safe, injectable access to settings instead of reading raw string keys " +
                "everywhere.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Strongly-typed, injectable access to a bound section of configuration", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "A way to make controller actions optional",                            isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "A caching layer for database queries",                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "A mechanism for handling HTTP OPTIONS requests",                       isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q27_Cors(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "What problem does CORS (Cross-Origin Resource Sharing) configuration address?",
            explanation:
                "Browsers enforce the same-origin policy, blocking JavaScript from calling an API on a " +
                "different origin (scheme/host/port) unless the server opts in. CORS configuration on the API " +
                "sends the headers that tell the browser which origins, methods, and headers are allowed — " +
                "letting a front-end on another origin call it.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "It lets a browser front-end on a different origin call the API, relaxing the same-origin policy", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "It encrypts the request body in transit",                                                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "It authenticates users with JWT tokens",                                                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "It compresses HTTP responses to reduce bandwidth",                                                 isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q28_ModelValidation(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Medium,
            text: "In a controller marked with `[ApiController]`, what happens when a request fails model validation (e.g. a `[Required]` field is missing)?",
            explanation:
                "`[ApiController]` enables automatic model validation: if `ModelState` is invalid, the framework " +
                "short-circuits and returns a 400 Bad Request with a validation problem-details body before the " +
                "action runs. Without `[ApiController]` you would check `ModelState.IsValid` manually.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "The framework automatically returns 400 Bad Request before the action executes", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "The action runs anyway and must check ModelState itself",                        isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The request is silently accepted with default values",                           isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "The server returns 500 Internal Server Error",                                    isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q29_AllowAnonymous(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Easy,
            text: "What does the `[AllowAnonymous]` attribute do?",
            explanation:
                "`[AllowAnonymous]` exempts an action or controller from authorization, allowing unauthenticated " +
                "access even when a global or controller-level `[Authorize]` policy is in effect. It's used for " +
                "endpoints like login or public health checks.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Allows unauthenticated access, overriding an applicable [Authorize] requirement", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Hides the endpoint from API documentation",                                       isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "Disables HTTPS for that endpoint",                                                 isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Grants the user administrator privileges",                                         isCorrect: false, orderIndex: 3),
            ]);
    }

    private static Question Q30_MiddlewareOrder(Guid categoryId)
    {
        var qid = Guid.NewGuid();
        return Question.Create(
            id: qid,
            categoryId: categoryId,
            type: QuestionType.MultipleChoice,
            difficulty: Difficulty.Hard,
            text: "Why must `UseAuthentication` be registered before `UseAuthorization` in the pipeline?",
            explanation:
                "Middleware runs in registration order. Authentication establishes who the user is (populates " +
                "`HttpContext.User`); authorization then decides whether that user may proceed. If authorization " +
                "ran first, there would be no authenticated identity to evaluate, so the order is mandatory.",
            options:
            [
                new Option(Guid.NewGuid(), qid, "Authentication must establish the user's identity before authorization can evaluate it", isCorrect: true,  orderIndex: 0),
                new Option(Guid.NewGuid(), qid, "Authorization is slower, so it should run last for performance",                          isCorrect: false, orderIndex: 1),
                new Option(Guid.NewGuid(), qid, "The order does not matter; ASP.NET Core reorders them automatically",                    isCorrect: false, orderIndex: 2),
                new Option(Guid.NewGuid(), qid, "Authentication depends on the response produced by authorization",                        isCorrect: false, orderIndex: 3),
            ]);
    }
}
