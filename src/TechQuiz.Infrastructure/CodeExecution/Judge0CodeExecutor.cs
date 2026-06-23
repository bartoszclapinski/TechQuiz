using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Infrastructure.CodeExecution;

/// <summary>
/// <see cref="ICodeExecutor"/> backed by a self-hosted Judge0 instance (ADR-018).
/// Untrusted code is never run in this process — it is POSTed to Judge0, which
/// compiles and runs it inside its isolate/cgroups sandbox and returns the verdict.
/// Uses the synchronous <c>wait=true</c> endpoint (Judge0 is configured with
/// ENABLE_WAIT_RESULT=true), so one round-trip yields the finished result.
/// </summary>
public sealed class Judge0CodeExecutor(HttpClient httpClient, IOptions<Judge0Options> options)
    : ICodeExecutor
{
    private readonly Judge0Options _options = options.Value;

    public async Task<CodeExecutionResult> ExecuteCSharpAsync(
        CodeExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new SubmissionRequest(
            SourceCode: request.SourceCode,
            LanguageId: _options.CSharpLanguageId,
            Stdin: request.Stdin);

        using var response = await httpClient.PostAsJsonAsync(
            "submissions?base64_encoded=false&wait=true", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SubmissionResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Judge0 returned an empty submission response.");

        return new CodeExecutionResult(
            Status: body.Status?.Description ?? "Unknown",
            Stdout: body.Stdout,
            Stderr: body.Stderr,
            CompileOutput: body.CompileOutput,
            TimeSeconds: double.TryParse(body.Time, out var seconds) ? seconds : null,
            MemoryKb: body.Memory);
    }

    private sealed record SubmissionRequest(
        [property: JsonPropertyName("source_code")] string SourceCode,
        [property: JsonPropertyName("language_id")] int LanguageId,
        [property: JsonPropertyName("stdin")] string? Stdin);

    private sealed record SubmissionResponse(
        [property: JsonPropertyName("stdout")] string? Stdout,
        [property: JsonPropertyName("stderr")] string? Stderr,
        [property: JsonPropertyName("compile_output")] string? CompileOutput,
        [property: JsonPropertyName("message")] string? Message,
        [property: JsonPropertyName("time")] string? Time,
        [property: JsonPropertyName("memory")] int? Memory,
        [property: JsonPropertyName("status")] SubmissionStatus? Status);

    private sealed record SubmissionStatus(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("description")] string Description);
}
