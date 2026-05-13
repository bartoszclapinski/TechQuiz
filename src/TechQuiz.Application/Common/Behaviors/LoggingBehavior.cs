using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TechQuiz.Application.Common.Behaviors;

/// <summary>
/// Logs the request name + duration around each MediatR handler invocation.
/// Intentionally does NOT log payloads — they may contain PII or secrets (e.g. passwords
/// inside <c>RegisterCommand</c>). Payload logging is the API/request-logging layer's job.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation(
                "{Request} handled in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "{Request} failed after {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
