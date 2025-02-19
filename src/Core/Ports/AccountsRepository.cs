using Core.Domain;

namespace Core.Ports;

public interface AccountsRepository
{
    Task<Account?> FindByEmail(string email);
    Task<Account?> FindById(Guid id);
    Task Create(Account account);
    Task Update(Account account);
    Task UpdateAndFlush(Account account);
    Task Flush();
}
