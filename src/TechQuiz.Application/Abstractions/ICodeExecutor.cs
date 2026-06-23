namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Compiles and runs untrusted user code in an isolated sandbox (ADR-018).
/// The implementation never executes code in-process — it delegates to a
/// self-hosted Judge0 instance. C# is the only language supported at launch.
/// </summary>
public interface ICodeExecutor
{
    Task<CodeExecutionResult> ExecuteCSharpAsync(
        CodeExecutionRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Source to run, plus optional stdin fed to the program.</summary>
public sealed record CodeExecutionRequest(string SourceCode, string? Stdin = null);

/// <summary>
/// Outcome of a single run. <paramref name="Status"/> is the sandbox's verdict
/// (e.g. "Accepted", "Compilation Error", "Time Limit Exceeded"). Output streams
/// and metrics are null when not applicable (e.g. no stdout on a compile error).
/// </summary>
public sealed record CodeExecutionResult(
    string Status,
    string? Stdout,
    string? Stderr,
    string? CompileOutput,
    double? TimeSeconds,
    int? MemoryKb);
