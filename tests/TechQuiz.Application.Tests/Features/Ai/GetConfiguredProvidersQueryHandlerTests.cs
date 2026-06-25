using FluentAssertions;
using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class GetConfiguredProvidersQueryHandlerTests
{
    private readonly IAiKeyStore _keyStore = Substitute.For<IAiKeyStore>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Guid _userId = Guid.NewGuid();

    private GetConfiguredProvidersQueryHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new GetConfiguredProvidersQueryHandler(_keyStore, _userContext);
    }

    [Fact]
    public async Task Handle_ReturnsConfiguredKindsForCurrentUser()
    {
        var kinds = new[] { AiProviderKind.Anthropic, AiProviderKind.OpenAi };
        _keyStore.ListConfiguredAsync(_userId, Arg.Any<CancellationToken>()).Returns(kinds);

        var result = await CreateSut().Handle(
            new GetConfiguredProvidersQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(kinds);
        await _keyStore.Received(1).ListConfiguredAsync(_userId, Arg.Any<CancellationToken>());
    }
}
