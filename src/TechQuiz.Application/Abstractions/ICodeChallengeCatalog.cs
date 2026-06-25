using TechQuiz.Domain;

namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Source of the available <see cref="CodeChallenge"/>s. Backed by an in-memory seed for
/// now (ADR-018 defers persistence to a later iteration); the port lets the storage swap
/// to a repository without touching callers.
/// </summary>
public interface ICodeChallengeCatalog
{
    IReadOnlyList<CodeChallenge> GetAll();

    CodeChallenge? Find(Guid id);
}
