using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Application;
using TechQuiz.Application.Features.Ai;
using TechQuiz.Domain;

namespace TechQuiz.Api.ErrorHandling;

/// <summary>
/// Translates exceptions bubbling out of the MediatR pipeline into RFC 7807 ProblemDetails
/// responses with the correct HTTP status. Without this, every error path returns 500 (or,
/// in Development, the framework's HTML developer page). The mapping mirrors the semantics
/// the handlers express through exception types — see <see cref="Map"/>.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request failed: {ExceptionType} -> {StatusCode} on {Method} {Path}",
                exception.GetType().Name, status, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
        };

        AddErrors(problemDetails, exception);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        RegistrationFailedException => (StatusCodes.Status400BadRequest, "Registration failed"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        MissingAiKeyException => (StatusCodes.Status409Conflict, "AI key not configured"),
        ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        DomainException => (StatusCodes.Status409Conflict, "Conflict"),
        _ => (StatusCodes.Status500InternalServerError, "Server error"),
    };

    private static void AddErrors(ProblemDetails problemDetails, Exception exception)
    {
        switch (exception)
        {
            case ValidationException validation:
                problemDetails.Extensions["errors"] = validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;
            case RegistrationFailedException registration:
                problemDetails.Extensions["errors"] = registration.Errors;
                break;
        }
    }
}
