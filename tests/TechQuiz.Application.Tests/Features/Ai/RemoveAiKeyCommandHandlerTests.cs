using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class RemoveAiKeyCommandHandlerTests
{
    private readonly IAiKeyStore _keyStore = Substitute.For<IAiKeyStore>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Guid _userId = Guid.NewGuid();

    private RemoveAiKeyCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new RemoveAiKeyCommandHandler(_keyStore, _userContext);
    }

    [Fact]
    public async Task Handle_RemovesKeyForCurrentUserAndProvider()
    {
        await CreateSut().Handle(
            new RemoveAiKeyCommand(AiProviderKind.Gemini),
            CancellationToken.None);

        await _keyStore.Received(1).RemoveAsync(
            _userId, AiProviderKind.Gemini, Arg.Any<CancellationToken>());
    }
}
