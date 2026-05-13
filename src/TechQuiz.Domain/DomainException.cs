namespace TechQuiz.Domain;

/// <summary>
/// Base class for domain-rule violations. Distinct from <see cref="ArgumentException"/>
/// so callers can catch business-rule failures without snagging generic argument errors.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
