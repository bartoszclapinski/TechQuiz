namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Thrown when an AI provider returns a response that cannot be parsed into question
/// drafts — an empty body, a missing text block, or malformed JSON. Distinct from a
/// transport/HTTP failure so callers can tell "the model misbehaved" from "the call failed".
/// </summary>
public sealed class AiResponseException(string message) : Exception(message);
