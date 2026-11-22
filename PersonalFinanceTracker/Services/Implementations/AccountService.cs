using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Repositories.Interfaces;
using PersonalFinanceTracker.Services.Dtos;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<AccountResponseDto>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAccountsAsync();
        return accounts.Select(a => new AccountResponseDto(
            a.Id,
            a.Name,
            a.Type,
            a.CurrentBalance
        ));
    }

    public async Task<AccountResponseDto?> GetAccountByIdAsync(int id)
    {
        var account = await _accountRepository.GetAccountByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        return new AccountResponseDto(
            account.Id,
            account.Name,
            account.Type,
            account.CurrentBalance
        );
    }

    public async Task<AccountResponseDto> CreateAccountAsync(CreateAccountRequestDto request)
    {
        var account = new Account
        {
            Name = request.Name,
            Type = request.Type,
            CurrentBalance = request.CurrentBalance
        };

        var createdAccount = await _accountRepository.CreateAccountAsync(account);

        return new AccountResponseDto(
            createdAccount.Id,
            createdAccount.Name,
            createdAccount.Type,
            createdAccount.CurrentBalance
        );
    }
}
