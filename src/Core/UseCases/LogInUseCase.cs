using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class LogInUseCase(
    AccountsRepository accountsRepository,
    TokenService tokenService,
    PasswordVerifier passwordVerifier,
    DateTimeProvider dateTimeProvider
)
{
    public async Task<Result<TokenPairOutput>> Execute(string email, string password)
    {
        var account = await accountsRepository.FindByEmail(email);
        if (account is null)
        {
            return new NoSuch<Account>();
        }

        if (!passwordVerifier.Verify(password, account.Password))
        {
            return new InvalidCredentials();
        }

        var result = account.CreateSession(dateTimeProvider.Now());

        if (result is { IsFailure: true, Exception: AccountNotActivated })
        {
            return result.Exception;
        }

        var tokenPair = tokenService.CreateTokenPair(account, result.Value.SessionId, result.Value.Id);

        await accountsRepository.UpdateAndFlush(account);

        return tokenPair;
    }
}
