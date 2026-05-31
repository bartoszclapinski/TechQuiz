using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed class RegisterCommandHandler(
    IUserAccountService userAccount,
    IJwtTokenService jwt,
    IRefreshTokenIssuer refreshTokenIssuer,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RegisterCommand, AuthTokensDto>
{
    public async Task<AuthTokensDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userId = await userAccount.CreateAsync(request.Email, request.Password, cancellationToken);

        var accessToken = jwt.IssueAccessToken(userId, request.Email);
        var refreshToken = refreshTokenIssuer.Issue(userId, timeProvider.GetUtcNow());
        await refreshTokenRepository.AddAsync(refreshToken.Token, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.RawValue,
            refreshToken.Token.ExpiresAt);
    }
}
