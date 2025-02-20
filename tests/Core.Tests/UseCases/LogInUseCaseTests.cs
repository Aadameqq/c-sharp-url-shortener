using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class LogInUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingPlainPassword = "plain-password";
    private readonly TokenPairOutput generatedTokenPair = new("access-token", "refresh-token");

    private readonly Mock<PasswordVerifier> passwordVerifierMock = new();
    private readonly Mock<TokenService> tokenServiceMock = new();

    private readonly LogInUseCase useCase;

    public LogInUseCaseTests()
    {
        useCase = new LogInUseCase(
            accountsRepositoryMock.Object,
            tokenServiceMock.Object,
            passwordVerifierMock.Object,
            dateTimeProviderMock.Object
        );

        accountsRepositoryMock
            .Setup(x => x.FindByEmail(existingAccount.Email))
            .ReturnsAsync(existingAccount);

        passwordVerifierMock
            .Setup(x => x.Verify(existingPlainPassword, existingAccount.Password))
            .Returns(true);

        dateTimeProviderMock.Setup(x => x.Now()).Returns(DateTime.MinValue);

        tokenServiceMock
            .Setup(x => x.CreateTokenPair(existingAccount, It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(generatedTokenPair);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-email", existingAccount.Password);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenPasswordIsInvalid_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, "invalid-password");

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidCredentials>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenAccountHasNotBeenActivated_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsFailure);
        Assert.IsType<AccountNotActivated>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenCredentialsAreValidAndAccountHasBeenActivated_ShouldReturnCorrectTokens()
    {
        existingAccount.Activate();

        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsSuccess);
        Assert.Same(generatedTokenPair.AccessToken, result.Value.AccessToken);
        Assert.Same(generatedTokenPair.RefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task WhenCredentialsAreValidAndAccountHasBeenActivated_UpdateAccountAndCreateSession()
    {
        existingAccount.Activate();

        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        var sessionId = Guid.Empty;
        var tokenId = Guid.Empty;

        tokenServiceMock
            .Setup(t => t.CreateTokenPair(It.IsAny<Account>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Callback(
                (Account _, Guid s, Guid t) =>
                {
                    sessionId = s;
                    tokenId = t;
                }
            )
            .Returns(generatedTokenPair);

        await useCase.Execute(existingAccount.Email, existingPlainPassword);

        var createdSession = actualAccount.GetSessionCurrentToken(sessionId);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
        Assert.NotNull(createdSession);
        Assert.Equal(tokenId, createdSession.Id);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}
