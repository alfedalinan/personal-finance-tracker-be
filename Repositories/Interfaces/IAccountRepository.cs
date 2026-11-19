using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetAccountByIdAsync(int id);
    Task<IEnumerable<Account>> GetAllAccountsAsync();
    Task<Account> CreateAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeleteAccountAsync(int id);
}
