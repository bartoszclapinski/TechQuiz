namespace TechQuiz.Infrastructure.CodeExecution;

/// <summary>
/// Strongly-typed binding for the <c>Judge0:*</c> configuration section (ADR-018).
/// <see cref="BaseUrl"/> points at the self-hosted Judge0 server — inside Docker
/// that is <c>http://judge0-server:2358</c>, on the host it is
/// <c>http://localhost:2358</c>. Validated on startup in <c>AddInfrastructure</c>.
/// </summary>
public sealed class Judge0Options
{
    public const string SectionName = "Judge0";

    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Judge0 language id for C#. Default 51 = "C# (Mono 6.6.0.161)" in Judge0 1.13.x.
    /// </summary>
    public int CSharpLanguageId { get; set; } = 51;
}
