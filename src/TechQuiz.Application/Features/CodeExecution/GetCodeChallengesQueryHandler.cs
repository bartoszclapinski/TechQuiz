using MediatR;
using TechQuiz.Application.Abstractions;

namespace TechQuiz.Application.Features.CodeExecution;

public sealed class GetCodeChallengesQueryHandler(ICodeChallengeCatalog catalog)
    : IRequestHandler<GetCodeChallengesQuery, IReadOnlyList<CodeChallengeDto>>
{
    public Task<IReadOnlyList<CodeChallengeDto>> Handle(
        GetCodeChallengesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CodeChallengeDto> challenges = catalog.GetAll()
            .Select(c => new CodeChallengeDto(
                c.Id,
                c.Title,
                c.Difficulty.ToString(),
                c.Prompt,
                c.StarterCode))
            .ToList();

        return Task.FromResult(challenges);
    }
}
