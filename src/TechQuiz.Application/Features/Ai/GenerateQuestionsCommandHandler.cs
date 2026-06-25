using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Resolves the requested provider, loads the current user's key for it, and asks
/// it for draft questions. Pure orchestration over the provider seam — no
/// persistence, no provider-specific logic. A missing key is surfaced as
/// <see cref="MissingAiKeyException"/> (the user must configure one); other provider
/// errors (rate limit, auth, parse failure) propagate unchanged.
/// </summary>
public sealed class GenerateQuestionsCommandHandler(
    IAiProviderResolver resolver, IAiKeyStore keyStore, IUserContext userContext)
    : IRequestHandler<GenerateQuestionsCommand, GenerateQuestionsResult>
{
    public async Task<GenerateQuestionsResult> Handle(
        GenerateQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        var provider = resolver.Resolve(request.Provider);

        var apiKey = await keyStore.GetAsync(userContext.UserId, request.Provider, cancellationToken)
            ?? throw new MissingAiKeyException(request.Provider);

        var drafts = await provider.GenerateQuestionsAsync(
            new GenerateQuestionsRequest(request.Topic, request.Difficulty, request.Count),
            apiKey,
            cancellationToken);

        return new GenerateQuestionsResult(request.Provider, drafts);
    }
}
