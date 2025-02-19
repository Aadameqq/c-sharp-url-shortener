using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class CreateAccountUseCase(
    AccountsRepository accountsRepository,
    PasswordHasher passwordHasher,
    ActivationCodesRepository activationCodesRepository,
    ActivationCodeEmailSender codeEmailSender
)
{
    public async Task<Result> Execute(string userName, string email, string plainPassword)
    {
        var found = await accountsRepository.FindByEmail(email);

        if (found != null)
        {
            return new AlreadyExists<Account>();
        }

        var hashedPassword = passwordHasher.HashPassword(plainPassword);

        var account = new Account(userName, email, hashedPassword);

        await accountsRepository.Create(account);

        var code = await activationCodesRepository.Create(account);

        codeEmailSender.Send(account, code);

        await accountsRepository.Flush();

        return Result.Success();
    }
}
