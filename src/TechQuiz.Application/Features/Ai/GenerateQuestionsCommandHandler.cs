using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Ai;

/// <summary>
/// Resolves the requested provider, loads the current user's key for it, asks it for draft
/// questions, and persists each draft to the pool in <see cref="PooledQuestionStatus.Draft"/>
/// (ADR-020) before returning the answer-key-free summaries. A missing key is surfaced as
/// <see cref="MissingAiKeyException"/> (the user must configure one); other provider errors
/// (rate limit, auth, parse failure) propagate unchanged, before anything is persisted.
/// </summary>
public sealed class GenerateQuestionsCommandHandler(
    IAiProviderResolver resolver,
    IAiKeyStore keyStore,
    IUserContext userContext,
    IPooledQuestionRepository pooledQuestions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
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

        var generatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var pooled = drafts
            .Select(draft => ToPooledQuestion(request, draft, generatedAtUtc))
            .ToList();

        await pooledQuestions.AddRangeAsync(pooled, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var summaries = pooled
            .Select(q => new GeneratedQuestionSummary(
                q.Id,
                q.Text,
                q.Options.OrderBy(o => o.OrderIndex).Select(o => o.Text).ToList(),
                q.Difficulty,
                q.Explanation))
            .ToList();

        return new GenerateQuestionsResult(request.Provider, summaries);
    }

    private PooledQuestion ToPooledQuestion(
        GenerateQuestionsCommand request, GeneratedQuestionDraft draft, DateTime generatedAtUtc)
    {
        var options = draft.Options
            .Select((text, index) => new PooledQuestionOption(
                Guid.NewGuid(), text, isCorrect: index == draft.CorrectOptionIndex, index))
            .ToList();

        return PooledQuestion.Create(
            Guid.NewGuid(),
            userContext.UserId,
            request.Provider.ToString(),
            request.Topic,
            generatedAtUtc,
            QuestionType.MultipleChoice,
            draft.Difficulty,
            draft.Stem,
            draft.Explanation,
            options);
    }
}
