using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Runs a user-submitted C# snippet in the sandbox and returns its verdict.
/// Spike scope (ADR-018): no hidden test harness yet — just compile + run +
/// capture output. The harness/grading layer is added in a later iteration.
/// </summary>
public sealed record RunCodeCommand(string SourceCode, string? Stdin = null)
    : IRequest<CodeExecutionResult>;
