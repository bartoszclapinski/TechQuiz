using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Pool;

/// <summary>
/// Loads the caller's draft and publishes it. Ownership is enforced — a user can only publish
/// their own draft (else <see cref="ForbiddenAccessException"/>). A missing draft is a
/// <see cref="KeyNotFoundException"/>; re-publishing surfaces the domain's
/// <c>PooledQuestionAlreadyPublishedException</c> unchanged.
/// </summary>
public sealed class PublishPooledQuestionCommandHandler(
    IPooledQuestionRepository pooledQuestions,
    IUserContext userContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PublishPooledQuestionCommand>
{
    public async Task Handle(PublishPooledQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await pooledQuestions.GetByIdAsync(request.PooledQuestionId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"PooledQuestion {request.PooledQuestionId} not found.");

        if (question.CreatedByUserId != userContext.UserId)
        {
            throw new ForbiddenAccessException(
                "PooledQuestion does not belong to the current user.");
        }

        question.Publish();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
