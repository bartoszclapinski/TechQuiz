using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Auth;
using TechQuiz.Domain;

namespace TechQuiz.Application.Tests.Features.Auth;

public class RefreshCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = Substitute.For<IRefreshTokenIssuer>();
    private readonly IJwtTokenService _jwt = Substitute.For<IJwtTokenService>();
    private readonly IUserAccountService _userAccount = Substitute.For<IUserAccountService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private static readonly DateTimeOffset T0 = new(2026, 5, 21, 14, 0, 0, TimeSpan.Zero);

    private RefreshCommandHandler CreateSut() =>
        new(_refreshTokenRepository, _refreshTokenIssuer, _jwt, _userAccount, _unitOfWork, _timeProvider);

    [Fact]
    public async Task Handle_HappyPath_RotatesTokens()
    {
        var userId = Guid.NewGuid();
        var oldToken = RefreshToken.Issue(Guid.NewGuid(), userId, "old-hash", T0.AddDays(-1), TimeSpan.FromDays(14));
        var newToken = RefreshToken.Issue(Guid.NewGuid(), userId, "new-hash", T0, TimeSpan.FromDays(14));
        var issued = new IssuedRefreshToken(newToken, "new-raw-value");
        var access = new AccessTokenResult("access-value", T0.AddMinutes(15));

        _refreshTokenRepository.FindByTokenAsync("old-raw-value", Arg.Any<CancellationToken>()).Returns(oldToken);
        _timeProvider.GetUtcNow().Returns(T0);
        _refreshTokenIssuer.Issue(userId, T0).Returns(issued);
        _userAccount.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new UserAccount(userId, "user@test.local"));
        _jwt.IssueAccessToken(userId, "user@test.local").Returns(access);

        var result = await CreateSut().Handle(new RefreshCommand("old-raw-value"), CancellationToken.None);

        result.AccessToken.Should().Be("access-value");
        // The DTO hands the client the raw value, never the persisted hash.
        result.RefreshToken.Should().Be("new-raw-value");

        // Rotation: the old token is revoked and the new one is persisted.
        oldToken.RevokedAt.Should().Be(T0);
        await _refreshTokenRepository.Received(1).AddAsync(newToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TokenNotFound_ThrowsUnauthorized()
    {
        _refreshTokenRepository.FindByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var act = async () => await CreateSut().Handle(new RefreshCommand("ghost"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*invalid*");
        await _refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorized_AndDoesNotRotate()
    {
        var expired = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "expired-value", T0.AddDays(-30), TimeSpan.FromDays(14));
        _refreshTokenRepository.FindByTokenAsync("expired-value", Arg.Any<CancellationToken>()).Returns(expired);
        _timeProvider.GetUtcNow().Returns(T0);

        var act = async () => await CreateSut().Handle(new RefreshCommand("expired-value"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired or revoked*");
        expired.RevokedAt.Should().BeNull(); // handler must not revoke an already-inactive token
        await _refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorized()
    {
        var revoked = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "revoked-value", T0.AddDays(-1), TimeSpan.FromDays(14));
        revoked.Revoke(T0.AddHours(-1));
        _refreshTokenRepository.FindByTokenAsync("revoked-value", Arg.Any<CancellationToken>()).Returns(revoked);
        _timeProvider.GetUtcNow().Returns(T0);

        var act = async () => await CreateSut().Handle(new RefreshCommand("revoked-value"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await _refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_UserMissing_ThrowsInvalidOperation_DataCorruptionSignal()
    {
        var userId = Guid.NewGuid();
        var oldToken = RefreshToken.Issue(Guid.NewGuid(), userId, "value", T0.AddDays(-1), TimeSpan.FromDays(14));
        _refreshTokenRepository.FindByTokenAsync("value", Arg.Any<CancellationToken>()).Returns(oldToken);
        _timeProvider.GetUtcNow().Returns(T0);
        _refreshTokenIssuer.Issue(userId, T0)
            .Returns(new IssuedRefreshToken(
                RefreshToken.Issue(Guid.NewGuid(), userId, "new-hash", T0, TimeSpan.FromDays(14)), "new-raw"));
        _userAccount.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserAccount?)null);

        var act = async () => await CreateSut().Handle(new RefreshCommand("value"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing user*");
    }
}
