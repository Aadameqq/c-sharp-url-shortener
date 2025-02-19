using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class AssignRoleUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid issuerId, Guid accountId, Role role)
    {
        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        var result = account.AssignRole(role, issuerId);

        if (result is { IsFailure: true, Exception: CannotManageOwn<Role> or RoleAlreadyAssigned })
        {
            return result.Exception;
        }

        await accountsRepository.UpdateAndFlush(account);

        return Result.Success();
    }
}
