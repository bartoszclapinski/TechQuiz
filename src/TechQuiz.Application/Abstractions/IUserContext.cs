namespace TechQuiz.Application.Abstractions;

/// <summary>
/// Provides the identity of the user making the current request.
/// Implemented in Infrastructure by reading JWT claims (iteration 1.3).
/// </summary>
public interface IUserContext
{
    Guid UserId { get; }
}
