using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class InitializePasswordResetUseCase(
    AccountsRepository accountsRepository,
    PasswordResetCodesRepository codesRepository,
    PasswordResetEmailSender emailSender
)
{
    public async Task<Result> Execute(string email)
    {
        var account = await accountsRepository.FindByEmail(email);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        var code = await codesRepository.Create(account);

        _ = emailSender.Send(account, code);

        return Result.Success();
    }
}
