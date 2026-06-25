using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.Ai;

public sealed class GetConfiguredProvidersQueryHandler(IAiKeyStore keyStore, IUserContext userContext)
    : IRequestHandler<GetConfiguredProvidersQuery, IReadOnlyList<AiProviderKind>>
{
    public Task<IReadOnlyList<AiProviderKind>> Handle(
        GetConfiguredProvidersQuery request, CancellationToken cancellationToken) =>
        keyStore.ListConfiguredAsync(userContext.UserId, cancellationToken);
}
