using System.Runtime.CompilerServices;

// Lets the integration test project assert on internal seed constants
// (DataSeeder.DemoUserEmail, etc.) without exposing them on the public API surface.
[assembly: InternalsVisibleTo("TechQuiz.Infrastructure.Tests")]
