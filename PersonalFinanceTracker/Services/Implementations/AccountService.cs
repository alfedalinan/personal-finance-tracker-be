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
            a.CurrentBalance,
            a.Slug
        ));
    }

    public async Task<IEnumerable<AccountWithRecentTransactionsResponseDto>> GetAllAccountsWithRecentTransactionsAsync()
    {
        var accounts = await _accountRepository.GetAllAccountsWithRecentTransactionsAsync();
        return accounts.Select(a => new AccountWithRecentTransactionsResponseDto(
            a.Id,
            a.Name,
            a.Type,
            a.CurrentBalance,
            a.Slug,
            a.Transactions.Select(t => new TransactionSummaryDto(
                t.Id,
                t.DateOfTransaction,
                t.Amount,
                t.BeforeBalance,
                t.AfterBalance,
                t.Description,
                t.Category?.Name ?? string.Empty,
                t.Category?.Type.ToString() ?? string.Empty
            ))
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
            account.CurrentBalance,
            account.Slug
        );
    }

    public async Task<AccountResponseDto?> GetAccountBySlugAsync(string slug)
    {
        var account = await _accountRepository.GetAccountBySlugAsync(slug);
        if (account == null)
        {
            return null;
        }

        return new AccountResponseDto(
            account.Id,
            account.Name,
            account.Type,
            account.CurrentBalance,
            account.Slug
        );
    }

    public async Task<AccountResponseDto?> GetAccountByTypeAsync(string type)
    {
        var account = await _accountRepository.GetAccountByTypeAsync(type);
        if (account == null)
        {
            return null;
        }

        return new AccountResponseDto(
            account.Id,
            account.Name,
            account.Type,
            account.CurrentBalance,
            account.Slug
        );
    }

    public async Task<AccountResponseDto> CreateAccountAsync(CreateAccountRequestDto request)
    {
        var slug = !string.IsNullOrWhiteSpace(request.Slug)
            ? request.Slug
            : GenerateSlug(request.Name);

        var account = new Account
        {
            Name = request.Name,
            Type = request.Type,
            Slug = slug,
            CurrentBalance = request.CurrentBalance
        };

        var createdAccount = await _accountRepository.CreateAccountAsync(account);

        return new AccountResponseDto(
            createdAccount.Id,
            createdAccount.Name,
            createdAccount.Type,
            createdAccount.CurrentBalance,
            createdAccount.Slug
        );
    }

    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "account";
        return name.ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}
