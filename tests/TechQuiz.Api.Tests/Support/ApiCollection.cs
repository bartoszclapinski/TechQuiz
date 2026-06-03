namespace TechQuiz.Api.Tests.Support;

/// <summary>
/// Shares one <see cref="TechQuizApiFactory"/> (and its Postgres container) across every
/// test class tagged with <c>[Collection(Name)]</c> — one host boot and one migration pass
/// per test run, rather than per class.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<TechQuizApiFactory>
{
    public const string Name = "Api";
}
