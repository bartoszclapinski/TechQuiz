using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

public sealed class SetAiKeyCommandHandler(IAiKeyStore keyStore, IUserContext userContext)
    : IRequestHandler<SetAiKeyCommand>
{
    public Task Handle(SetAiKeyCommand request, CancellationToken cancellationToken) =>
        keyStore.UpsertAsync(
            userContext.UserId, request.Provider, request.ApiKey, cancellationToken);
}
