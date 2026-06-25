using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

public sealed class RemoveAiKeyCommandHandler(IAiKeyStore keyStore, IUserContext userContext)
    : IRequestHandler<RemoveAiKeyCommand>
{
    public Task Handle(RemoveAiKeyCommand request, CancellationToken cancellationToken) =>
        keyStore.RemoveAsync(userContext.UserId, request.Provider, cancellationToken);
}
