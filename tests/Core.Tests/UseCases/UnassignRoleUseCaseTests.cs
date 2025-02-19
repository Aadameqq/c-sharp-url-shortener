using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class UnassignRoleUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");

    private readonly UnassignRoleUseCase useCase;

    public UnassignRoleUseCaseTests()
    {
        useCase = new UnassignRoleUseCase(accountsRepositoryMock.Object);

        accountsRepositoryMock
            .Setup(r => r.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);
    }

    [Fact]
    public async Task ShouldFail_WhenAccountWithGivenIdDoesNotExist()
    {
        var result = await useCase.Execute(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountAttemptsToSelfUnassignRole()
    {
        var result = await useCase.Execute(existingAccount.Id, existingAccount.Id);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldSucceed_WhenAccountWithGivenIdExistsAndIsNotIssuer()
    {
        var result = await useCase.Execute(Guid.NewGuid(), existingAccount.Id);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ShouldPersistCorrectAccount_WhenAccountWithGivenIdExistsAndIsNotIssuer()
    {
        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(Guid.NewGuid(), existingAccount.Id);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(r => r.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}
