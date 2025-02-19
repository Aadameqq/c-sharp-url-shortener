using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class ResetPasswordUseCase(
    PasswordHasher passwordHasher,
    AccountsRepository accountsRepository,
    PasswordResetCodesRepository passwordResetCodesRepository
)
{
    public async Task<Result> Execute(string resetCode, string newPassword)
    {
        var accountId = await passwordResetCodesRepository.GetAccountIdAndRevokeCode(resetCode);

        if (accountId is null)
        {
            return new NoSuch();
        }

        var account = await accountsRepository.FindById(accountId.Value);

        if (account is null)
        {
            return new NoSuch();
        }

        var passwordHash = passwordHasher.HashPassword(newPassword);

        account.ResetPassword(passwordHash);

        await accountsRepository.UpdateAndFlush(account);

        return Result.Success();
    }
}
