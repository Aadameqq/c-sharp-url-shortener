using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class ActivateAccountUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<ActivationCodesRepository> activationCodesRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingCode = "123";

    private readonly ActivateAccountUseCase useCase;

    public ActivateAccountUseCaseTests()
    {
        useCase = new ActivateAccountUseCase(
            activationCodesRepositoryMock.Object,
            accountsRepositoryMock.Object
        );

        activationCodesRepositoryMock
            .Setup(x => x.GetAccountIdAndRevokeCode(existingCode))
            .ReturnsAsync(existingAccount.Id);

        accountsRepositoryMock
            .Setup(x => x.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);
    }

    [Fact]
    public async Task WhenCodeDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-code");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenUserAssignedToCodeDoesNotExist_ShouldFail()
    {
        activationCodesRepositoryMock
            .Setup(x => x.GetAccountIdAndRevokeCode(existingCode))
            .ReturnsAsync(Guid.Empty);

        var result = await useCase.Execute(existingCode);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenCodeIsValid_ShouldActivateAccountAndPersistIt()
    {
        Account? actualAccount = null;

        accountsRepositoryMock
            .Setup(x => x.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account account) => actualAccount = account);

        var result = await useCase.Execute(existingCode);

        Assert.True(result.IsSuccess);
        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
        Assert.True(actualAccount.HasBeenActivated());
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}
