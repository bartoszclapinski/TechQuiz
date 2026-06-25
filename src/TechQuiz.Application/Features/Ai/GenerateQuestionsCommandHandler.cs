using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Resolves the requested provider and asks it for draft questions. Pure
/// orchestration over the provider seam — no persistence, no provider-specific
/// logic. Errors from the provider (rate limit, auth, parse failure) propagate
/// unchanged so the caller can react rather than receiving an empty result.
/// </summary>
public sealed class GenerateQuestionsCommandHandler(IAiProviderResolver resolver)
    : IRequestHandler<GenerateQuestionsCommand, GenerateQuestionsResult>
{
    public async Task<GenerateQuestionsResult> Handle(
        GenerateQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        var provider = resolver.Resolve(request.Provider);

        var drafts = await provider.GenerateQuestionsAsync(
            new GenerateQuestionsRequest(request.Topic, request.Difficulty, request.Count),
            cancellationToken);

        return new GenerateQuestionsResult(request.Provider, drafts);
    }
}
