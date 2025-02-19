using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class UnassignRoleUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid issuerId, Guid accountId)
    {
        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        var result = account.RemoveRole(issuerId);

        if (result is { IsFailure: true, Exception: CannotManageOwn<Role> })
        {
            return result.Exception;
        }

        await accountsRepository.UpdateAndFlush(account);

        return Result.Success();
    }
}
