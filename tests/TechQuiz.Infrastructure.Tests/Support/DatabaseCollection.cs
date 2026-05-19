namespace TechQuiz.Infrastructure.Tests.Support;

/// <summary>
/// xUnit collection definition that shares one <see cref="PostgresContainerFixture"/>
/// across every test class tagged with <c>[Collection(Name)]</c>. One container, one
/// migration pass per test run — tests pay only the truncate cost between methods.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Database";
}
