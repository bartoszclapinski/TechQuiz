using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed class LoginCommandHandler(
    IUserAccountService userAccount,
    IJwtTokenService jwt,
    IRefreshTokenIssuer refreshTokenIssuer,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LoginCommand, AuthTokensDto>
{
    public async Task<AuthTokensDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = await userAccount.VerifyCredentialsAsync(request.Email, request.Password, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var accessToken = jwt.IssueAccessToken(account.Id, account.Email);
        var refreshToken = refreshTokenIssuer.Issue(account.Id, timeProvider.GetUtcNow());
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }
}
