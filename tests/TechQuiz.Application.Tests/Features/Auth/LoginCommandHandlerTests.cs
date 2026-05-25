using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Auth;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserAccountService _userAccount = Substitute.For<IUserAccountService>();
    private readonly IJwtTokenService _jwt = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = Substitute.For<IRefreshTokenIssuer>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 21, 14, 0, 0, TimeSpan.Zero);

    private LoginCommandHandler CreateSut() =>
        new(_userAccount, _jwt, _refreshTokenIssuer, _refreshTokenRepository, _unitOfWork, _timeProvider);

    [Fact]
    public async Task Handle_HappyPath_ReturnsTokenPair()
    {
        var userId = Guid.NewGuid();
        var refresh = RefreshToken.Issue(Guid.NewGuid(), userId, "refresh-value", T0, TimeSpan.FromDays(14));
        var access = new AccessTokenResult("access-value", T0.AddMinutes(15));

        _userAccount.VerifyCredentialsAsync("user@test.local", "Password1!", Arg.Any<CancellationToken>())
            .Returns(userId);
        _timeProvider.GetUtcNow().Returns(T0);
        _refreshTokenIssuer.Issue(userId, T0).Returns(refresh);
        _jwt.IssueAccessToken(userId, "user@test.local").Returns(access);

        var result = await CreateSut().Handle(
            new LoginCommand("user@test.local", "Password1!"), CancellationToken.None);

        result.AccessToken.Should().Be("access-value");
        result.RefreshToken.Should().Be("refresh-value");
        await _refreshTokenRepository.Received(1).AddAsync(refresh, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorized_AndDoesNotIssueTokens()
    {
        _userAccount.VerifyCredentialsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var act = async () => await CreateSut().Handle(
            new LoginCommand("user@test.local", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _jwt.DidNotReceiveWithAnyArgs().IssueAccessToken(default, default!);
        _refreshTokenIssuer.DidNotReceiveWithAnyArgs().Issue(default, default);
        await _refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
