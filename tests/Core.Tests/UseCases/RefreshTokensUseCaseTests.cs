using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class RefreshTokensUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly Guid existingCurrentTokenId;
    private readonly Guid existingSessionId;
    private readonly TokenPairOutput existingTokenPair = new("access", "refresh");

    private readonly Mock<TokenService> tokenServiceMock = new();

    private readonly RefreshTokensUseCase useCase;
    private readonly string validToken = "validToken";

    public RefreshTokensUseCaseTests()
    {
        useCase = new RefreshTokensUseCase(
            accountsRepositoryMock.Object,
            dateTimeProviderMock.Object,
            tokenServiceMock.Object
        );
        existingAccount.Activate();

        var created = existingAccount.CreateSession(DateTime.MinValue).Value;
        existingSessionId = created.SessionId;
        existingCurrentTokenId = created.Id;

        var validTokenPayload = new RefreshTokenPayload(existingAccount.Id, existingCurrentTokenId, existingSessionId);

        tokenServiceMock.Setup(s => s.FetchRefreshTokenPayloadIfValid(validToken)).ReturnsAsync(validTokenPayload);

        accountsRepositoryMock.Setup(r => r.FindById(existingAccount.Id)).ReturnsAsync(existingAccount);

        tokenServiceMock.Setup(s => s.CreateTokenPair(existingAccount, existingSessionId, It.IsAny<Guid>()))
            .Returns(existingTokenPair);
    }

    [Fact]
    public async Task ShouldFail_WhenGivenTokenIsInvalid()
    {
        var result = await useCase.Execute("invalid-token");

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidToken>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountWithIdFromPayloadDoesNotExist()
    {
        var payloadWithInvalidAccount = new RefreshTokenPayload(Guid.Empty, existingCurrentTokenId, existingSessionId);

        tokenServiceMock.Setup(s => s.FetchRefreshTokenPayloadIfValid(validToken))
            .ReturnsAsync(payloadWithInvalidAccount);

        var result = await useCase.Execute(validToken);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenSessionLinkedToTokenDoesNotExist()
    {
        var payloadWithInvalidSession = new RefreshTokenPayload(existingAccount.Id, existingCurrentTokenId, Guid.Empty);

        tokenServiceMock.Setup(s => s.FetchRefreshTokenPayloadIfValid(validToken))
            .ReturnsAsync(payloadWithInvalidSession);

        var result = await useCase.Execute(validToken);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
    }

    [Fact]
    public async Task ShouldFail_WhenSessionLinkedToTokenExistsButGivenTokenIsNotCurrentOne()
    {
        var payloadWithTokenNotBeingCurrentOne =
            new RefreshTokenPayload(existingAccount.Id, Guid.Empty, existingSessionId);

        tokenServiceMock.Setup(s => s.FetchRefreshTokenPayloadIfValid(validToken))
            .ReturnsAsync(payloadWithTokenNotBeingCurrentOne);

        var result = await useCase.Execute(validToken);

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidToken>(result.Exception);
    }

    [Fact]
    public async Task ShouldSucceedAndReturnCorrectPair_WhenAllRequirementsAreFulfilled()
    {
        var result = await useCase.Execute(validToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equivalent(existingTokenPair, result.Value);
    }

    [Fact]
    public async Task ShouldPersistAccountWithUpdatedSession_WhenAllRequirementsAreFulfilled()
    {
        Account actualAccount = null!;
        var newTokenId = Guid.Empty;

        accountsRepositoryMock.Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        tokenServiceMock.Setup(s => s.CreateTokenPair(existingAccount, existingSessionId, It.IsAny<Guid>()))
            .Callback((Account _, Guid _, Guid t) => newTokenId = t).Returns(existingTokenPair);

        await useCase.Execute(validToken);

        var actualToken = existingAccount.GetSessionCurrentToken(existingSessionId);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
        Assert.NotNull(actualToken);
        Assert.Equal(newTokenId, actualToken.Id);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(r => r.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}
