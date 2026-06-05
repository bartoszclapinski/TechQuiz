namespace TechQuiz.Api.Contracts.Auth;

// RefreshToken is optional: the browser flow carries it in an HttpOnly cookie, so the
// body is empty there. API clients (Postman, tests) without a cookie jar send it here.
public sealed record RefreshRequest(string? RefreshToken);
