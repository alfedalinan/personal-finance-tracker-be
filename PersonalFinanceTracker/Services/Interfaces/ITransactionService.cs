using PersonalFinanceTracker.Entities;
using PersonalFinanceTracker.Services.Dtos;

namespace PersonalFinanceTracker.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionSummaryDto> CreateTransactionAsync(CreateTransactionRequestDto request);
    Task<TransactionSummaryDto> UpdateTransactionAsync(int id, CreateTransactionRequestDto request);
    Task DeleteTransactionAsync(int id);
    Task<AccountTransactionsResponseDto?> GetAccountTransactionsAndBalanceAsync(int accountId);
}
