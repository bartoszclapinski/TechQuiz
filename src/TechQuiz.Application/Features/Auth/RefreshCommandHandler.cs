using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed class RefreshCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenIssuer refreshTokenIssuer,
    IJwtTokenService jwt,
    IUserAccountService userAccount,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshCommand, AuthTokensDto>
{
    public async Task<AuthTokensDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existing = await refreshTokenRepository.FindByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token is invalid.");

        var now = timeProvider.GetUtcNow();
        if (!existing.IsActiveAt(now))
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        // Refresh-token rotation: every refresh produces a new token pair and the old
        // refresh token is single-use. If the same refresh value ever shows up again,
        // the second hit fails the IsActiveAt check above (RevokedAt is set).
        existing.Revoke(now);

        var newRefresh = refreshTokenIssuer.Issue(existing.UserId, now);
        await refreshTokenRepository.AddAsync(newRefresh.Token, cancellationToken);

        var user = await userAccount.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Refresh token references missing user {existing.UserId} — possible FK corruption.");

        var accessToken = jwt.IssueAccessToken(user.Id, user.Email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefresh.RawValue,
            newRefresh.Token.ExpiresAt);
    }
}
