using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Features.CodeExecution;

/// <summary>
/// Asks the caller's chosen AI provider for prose feedback on a code-challenge submission
/// (ADR-018). It loads the challenge (unknown id → 404), resolves the provider, and loads the
/// caller's own key (missing → <see cref="MissingAiKeyException"/> → 409, bring-your-own-key,
/// ADR-006). The hidden test cases are handed to the provider so the model can reason about
/// missed edge cases; the provider prompt keeps it from leaking their exact expected outputs.
/// The feedback is explicitly complementary — the deterministic grade still owns pass/fail.
/// </summary>
public sealed class GetCodeFeedbackCommandHandler(
    ICodeChallengeCatalog catalog,
    IAiProviderResolver resolver,
    IAiKeyStore keyStore,
    IUserContext userContext)
    : IRequestHandler<GetCodeFeedbackCommand, GetCodeFeedbackResult>
{
    public async Task<GetCodeFeedbackResult> Handle(
        GetCodeFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var challenge = catalog.Find(request.ChallengeId)
            ?? throw new KeyNotFoundException(
                $"CodeChallenge '{request.ChallengeId}' was not found.");

        var provider = resolver.Resolve(request.Provider);

        var apiKey = await keyStore.GetAsync(userContext.UserId, request.Provider, cancellationToken)
            ?? throw new MissingAiKeyException(request.Provider);

        var feedbackRequest = new CodeFeedbackRequest(
            challenge.Title,
            challenge.Prompt,
            request.SourceCode,
            challenge.TestCases
                .OrderBy(tc => tc.OrderIndex)
                .Select(tc => new CodeFeedbackTestCase(tc.Stdin, tc.ExpectedStdout))
                .ToList());

        var feedback = await provider.GenerateCodeFeedbackAsync(
            feedbackRequest, apiKey, cancellationToken);

        return new GetCodeFeedbackResult(feedback, request.Provider);
    }
}
