using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class AssignRoleUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");

    private readonly AssignRoleUseCase useCase;

    public AssignRoleUseCaseTests()
    {
        useCase = new AssignRoleUseCase(accountsRepositoryMock.Object);

        accountsRepositoryMock
            .Setup(r => r.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);
    }

    [Fact]
    public async Task ShouldFail_WhenAccountWithGivenIdDoesNotExist()
    {
        var result = await useCase.Execute(Guid.NewGuid(), Guid.Empty, Role.Admin);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountAttemptsToSelfAssignRole()
    {
        var result = await useCase.Execute(existingAccount.Id, existingAccount.Id, Role.Admin);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountAlreadyHasRole()
    {
        existingAccount.AssignRole(Role.ProblemsCreator, Guid.NewGuid());

        var result = await useCase.Execute(Guid.NewGuid(), existingAccount.Id, Role.Admin);

        Assert.True(result.IsFailure);
        Assert.IsType<RoleAlreadyAssigned>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldSucceed_WhenAccountExistsAndHasNoRole()
    {
        var result = await useCase.Execute(Guid.NewGuid(), existingAccount.Id, Role.Admin);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ShouldPersistAccount_WhenAccountExistsAndHasNoRole()
    {
        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(Guid.NewGuid(), existingAccount.Id, Role.Admin);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(r => r.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}
