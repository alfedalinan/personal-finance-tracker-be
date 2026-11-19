using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Services.Dtos;

namespace PersonalFinanceTracker.Services.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(CreateTransactionRequestDto request);
    Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId);
}
