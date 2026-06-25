using NSubstitute;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Features.Ai;

namespace TechQuiz.Application.Tests.Features.Ai;

public class SetAiKeyCommandHandlerTests
{
    private readonly IAiKeyStore _keyStore = Substitute.For<IAiKeyStore>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Guid _userId = Guid.NewGuid();

    private SetAiKeyCommandHandler CreateSut()
    {
        _userContext.UserId.Returns(_userId);
        return new SetAiKeyCommandHandler(_keyStore, _userContext);
    }

    [Fact]
    public async Task Handle_UpsertsKeyForCurrentUserAndProvider()
    {
        await CreateSut().Handle(
            new SetAiKeyCommand(AiProviderKind.Anthropic, "sk-ant-123"),
            CancellationToken.None);

        await _keyStore.Received(1).UpsertAsync(
            _userId, AiProviderKind.Anthropic, "sk-ant-123", Arg.Any<CancellationToken>());
    }
}
