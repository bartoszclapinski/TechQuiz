using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class RefreshTokenTests
{
    private static readonly Guid AnyTokenId = Guid.NewGuid();
    private static readonly Guid AnyUserId = Guid.NewGuid();
    private static readonly DateTimeOffset AnyIssuedAt = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan AnyLifetime = TimeSpan.FromDays(14);
    private const string AnyToken = "opaque-token-value";

    [Fact]
    public void Issue_WithValidInput_ReturnsTokenWithComputedExpiry()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);

        token.Id.Should().Be(AnyTokenId);
        token.UserId.Should().Be(AnyUserId);
        token.Token.Should().Be(AnyToken);
        token.IssuedAt.Should().Be(AnyIssuedAt);
        token.ExpiresAt.Should().Be(AnyIssuedAt + AnyLifetime);
        token.RevokedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Issue_WithEmptyToken_Throws(string? value)
    {
        var act = () => RefreshToken.Issue(AnyTokenId, AnyUserId, value!, AnyIssuedAt, AnyLifetime);

        act.Should().Throw<InvalidRefreshTokenException>()
            .WithMessage("*token value must not be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Issue_WithNonPositiveLifetime_Throws(int seconds)
    {
        var act = () => RefreshToken.Issue(
            AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, TimeSpan.FromSeconds(seconds));

        act.Should().Throw<InvalidRefreshTokenException>()
            .WithMessage("*lifetime must be positive*");
    }

    [Fact]
    public void IsActiveAt_ReturnsTrue_WhenWithinLifetimeAndNotRevoked()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);

        token.IsActiveAt(AnyIssuedAt + TimeSpan.FromHours(1)).Should().BeTrue();
    }

    [Fact]
    public void IsActiveAt_ReturnsFalse_WhenNowIsAtOrPastExpiry()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);
        var atExpiry = token.ExpiresAt;

        token.IsActiveAt(atExpiry).Should().BeFalse();
        token.IsActiveAt(atExpiry + TimeSpan.FromSeconds(1)).Should().BeFalse();
    }

    [Fact]
    public void IsActiveAt_ReturnsFalse_AfterRevoke()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);
        token.Revoke(AnyIssuedAt + TimeSpan.FromHours(1));

        token.IsActiveAt(AnyIssuedAt + TimeSpan.FromHours(2)).Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);
        var revokeTime = AnyIssuedAt + TimeSpan.FromHours(2);

        token.Revoke(revokeTime);

        token.RevokedAt.Should().Be(revokeTime);
    }

    [Fact]
    public void Revoke_OnAlreadyRevokedToken_Throws()
    {
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);
        token.Revoke(AnyIssuedAt + TimeSpan.FromHours(1));

        var act = () => token.Revoke(AnyIssuedAt + TimeSpan.FromHours(2));

        act.Should().Throw<RefreshTokenAlreadyRevokedException>();
    }

    [Fact]
    public void Revoke_OnExpiredButNotRevokedToken_Succeeds()
    {
        // Administrative bookkeeping: marking a token revoked after it has expired
        // is legitimate (e.g. audit trail). Expiry and revocation are independent.
        var token = RefreshToken.Issue(AnyTokenId, AnyUserId, AnyToken, AnyIssuedAt, AnyLifetime);
        var afterExpiry = token.ExpiresAt + TimeSpan.FromDays(1);

        var act = () => token.Revoke(afterExpiry);

        act.Should().NotThrow();
        token.RevokedAt.Should().Be(afterExpiry);
    }
}
