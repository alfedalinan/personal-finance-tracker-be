using PersonalFinanceTracker.Services.Dtos;

namespace PersonalFinanceTracker.Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountResponseDto>> GetAllAccountsAsync();
    Task<IEnumerable<AccountWithRecentTransactionsResponseDto>> GetAllAccountsWithRecentTransactionsAsync();
    Task<AccountResponseDto?> GetAccountByIdAsync(int id);
    Task<AccountResponseDto?> GetAccountBySlugAsync(string slug);
    Task<AccountResponseDto?> GetAccountByTypeAsync(string type);
    Task<AccountResponseDto> CreateAccountAsync(CreateAccountRequestDto request);
}
